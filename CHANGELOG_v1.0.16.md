# VanBanPlus v1.0.16

**Ngày phát hành:** 14/05/2026
**Loại bản cập nhật:** Cải tiến trải nghiệm cập nhật (không bắt buộc)

## ✨ Cải tiến chính

### 🆕 Cửa sổ tiến trình tải bản cập nhật
Trước đây khi nhấn **OK** trên thông báo cập nhật, ứng dụng âm thầm tải file mà không hiển thị bất cứ dấu hiệu nào → người dùng tưởng app bị treo.

Bản này thêm **cửa sổ tiến trình thân thiện** với:
- 📊 **Thanh ProgressBar** hiển thị % thực tế.
- 🔢 **Dung lượng đã tải / tổng** (ví dụ `12.45 MB / 53.87 MB`).
- ⚡ **Tốc độ tải** real-time (KB/s hoặc MB/s).
- ⏱️ **Thời gian còn lại** ước tính (`còn ~25s`).
- ⛔ **Nút "Hủy"** cho phép dừng tải bất cứ lúc nào.
- ✅ Trạng thái rõ ràng: "Đang kết nối...", "Đang tải dữ liệu từ GitHub...", "✅ Hoàn tất! Chuẩn bị cài đặt...".

### 🛡️ Xử lý lỗi tốt hơn
- Khi có lỗi mạng: hiển thị thông báo đỏ trong cửa sổ tiến trình + đề nghị mở trình duyệt tải thủ công.
- Khi người dùng hủy: xóa file tạm và thông báo "có thể cập nhật lại bất cứ lúc nào".

## 🔧 Kỹ thuật

- Bump version: `1.0.15` → `1.0.16` (csproj, AssemblyVersion, FileVersion).
- Mới: `AIVanBan.Desktop/Services/UpdateProgressWindow.cs` — cửa sổ tiến trình code-only WPF.
- Refactor: `AppUpdateService.DownloadAndRunInstallerAsync()` báo tiến trình mỗi 100ms (10 fps).
- `update.xml`: `mandatory=false`.
- Installer Inno Setup vẫn loại trừ `*.db, *.litedb, *.bak, settings.json` để cài sạch.

## ⬇️ Tải về

- **Setup (x64)**: `VanBanPlus-Setup-1.0.16-x64.exe` (~54 MB)

## 🔄 Cập nhật tự động

> ⚠️ **Lưu ý quan trọng:** Người dùng đang chạy v1.0.15 (hoặc cũ hơn) khi cập nhật lên v1.0.16 vẫn sẽ thấy **giao diện tải cũ** (vì code mới chỉ chạy trong v1.0.16+). Từ v1.0.16 trở đi, mọi lần cập nhật trong tương lai sẽ có cửa sổ tiến trình mới.
