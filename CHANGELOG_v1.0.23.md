# VanBanPlus v1.0.23

**Ngày phát hành:** 26/11/2026
**Loại bản cập nhật:** Sửa lỗi (không bắt buộc)

## 📥 Sổ văn bản

- **Sửa văn bản đã lưu:** đổi Loại văn bản không còn tự cấp lại Số và ký hiệu (trước đây mất số cũ, sai dữ liệu sổ).
- **Thêm văn bản đến:** ô Số, Cơ quan ban hành, Địa danh, Người ký, Chức danh để trống để nhập theo nơi gửi; trước đây điền sẵn theo văn bản đi của cơ quan mình. Chuyển lại «Văn bản đi» thì các ô trống được điền lại.
- **Loại mặc định** khi thêm mới là Công văn (trước đây là mục đầu danh sách «Luật»).
- **Nút lọc «Quá hạn»** dùng cùng quy tắc với thông báo khi mở máy và dòng tô đỏ: chỉ văn bản đến, hạn đã qua (hạn hôm nay chưa tính), chưa lưu trữ / ban hành.
- **Cột Hạn xử lý** rộng hơn, không cắt chữ «⚠ Quá hạn N ngày».
- **Thanh bên «Tổng: N văn bản»** không đếm văn bản trong thùng rác.

## 🤖 Soạn thảo theo mẫu (máy chủ, áp dụng cả bản cũ)

- Ô của mẫu chuyên ngành (Y tế, Giáo dục, UBND xã…) mà khuôn loại văn bản không có sẵn nay vẫn được gửi cho AI ở mục «THÔNG TIN BỔ SUNG». Trước đây 26/69 mẫu cài sẵn bị bỏ một phần hoặc toàn bộ ô đã điền. Sửa ở máy chủ (vanbanplus-api `acbebe0`), không cần cập nhật bản cài.

## 🔧 Kỹ thuật

- `DocumentEditDialog`: `_isNew` + giá trị tự điền (`_autoNumber`…), chỉ ghi đè ô còn đúng giá trị tự điền.
- `DocumentService.IsOverdue` (quy tắc quá hạn duy nhất), `GetTotalDocuments` lọc `IsDeleted`; bộ kiểm `DocumentOverdueTests`.
- Bump version: `1.0.22` → `1.0.23` (csproj, AssemblyVersion, FileVersion, Inno Setup). `update.xml`: `mandatory=false`.
