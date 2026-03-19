# ✅ CHECKLIST TEST FEEDBACK — VanBanPlus v1.0.13

> **Hướng dẫn:** Mỗi mục có 2 ô click:
>
> - Click ô **OK** nếu test thành công ✅
> - Click ô **LỖI** nếu test thất bại ❌ → ghi chi tiết vào phần "Ghi chú"
> - Không click gì = chưa test

---

## 🔴 FB-1: Xuất Word bị sai format

- 1.1 Quản lý VB → chọn VB → Xuất Word → font Times New Roman đúng
  - [ ] OK
  - [ ] LỖI
- 1.2 AI Soạn VB → tạo VB → Xuất Word → format đúng (lề, font, cỡ chữ)
  - [ ] OK
  - [ ] LỖI
- 1.3 AI Báo cáo → tạo BC → Xuất Word → format đúng
  - [ ] OK
  - [ ] LỖI
- 1.4 Cuộc họp → menu ⋮ → Xuất biên bản Word → format đúng
  - [ ] OK
  - [ ] LỖI

📝 Ghi chú lỗi FB-1:

---

## 🟠 FB-2: Mẫu Văn Bản — UX

- 2.1 Vào Mẫu VB → mỗi mẫu có nút "📝 Soạn VB" (không phải icon ▶)
  - [ ] OK
  - [ ] LỖI
- 2.2 Click 👁 Xem → font Times New Roman (không phải font code)
  - [ ] OK
  - [ ] LỖI
- 2.3 Trong dialog Xem → có nút "Sử dụng mẫu này"
  - [ ] OK
  - [ ] LỖI
- 2.4 Mỗi mẫu hiện badges (loại VB, số biến, tags)
  - [ ] OK
  - [ ] LỖI

📝 Ghi chú lỗi FB-2:

---

## 🟠 FB-3: Nút "Chọn tất cả" văn bản

- 3.1 Quản lý VB → toolbar có nút "Chọn tất cả"
  - [ ] OK
  - [ ] LỖI
- 3.2 Bấm "Chọn tất cả" → tất cả dòng chọn → hiện "Bỏ chọn"
  - [ ] OK
  - [ ] LỖI
- 3.3 Bấm Ctrl+A → chọn tất cả dòng
  - [ ] OK
  - [ ] LỖI

📝 Ghi chú lỗi FB-3:

---

## 🔵 FB-4: Trạng thái văn bản — badge + quick-switch

- 4.1 DataGrid có cột badge màu trạng thái (Nháp/Trình ký/Đã duyệt...)
  - [ ] OK
  - [ ] LỖI
- 4.2 Click badge → hiện menu 7 trạng thái → chọn → đổi thành công
  - [ ] OK
  - [ ] LỖI
- 4.3 Hover badge → tooltip giải thích trạng thái
  - [ ] OK
  - [ ] LỖI
- 4.4 Trạng thái hiện tại có dấu ✔ trong menu
  - [ ] OK
  - [ ] LỖI

📝 Ghi chú lỗi FB-4:

---

## 🟠 FB-5: Cuộc họp — Tạo nhanh + Lọc

- 5.1 "Thêm cuộc họp ▼" → "Tạo nhanh" → cửa sổ nhỏ, chỉ Tab 1
  - [ ] OK
  - [ ] LỖI
- 5.2 Có 4 nút lọc: Hôm nay / Tuần này / Tháng này / Sắp tới
  - [ ] OK
  - [ ] LỖI
- 5.3 Cuộc họp → menu ⋮ → "Lưu làm mẫu" → lưu OK
  - [ ] OK
  - [ ] LỖI
- 5.4 "Thêm ▼" → "Từ mẫu" → chọn mẫu → form tự điền sẵn
  - [ ] OK
  - [ ] LỖI

📝 Ghi chú lỗi FB-5:

---

## 🔴 FB-6: Lịch tổng hợp — hiển thị + tương tác

- 6.1 Tạo cuộc họp → vào Lịch → cuộc họp hiện đúng ngày
  - [ ] OK
  - [ ] LỖI
- 6.2 Click vào cuộc họp trên lịch → mở dialog sửa
  - [ ] OK
  - [ ] LỖI
- 6.3 Click ngày trống → mở Quick Create với ngày đã chọn
  - [ ] OK
  - [ ] LỖI
- 6.4 Toggle Tháng/Tuần → xem Tuần hiện time-slot 7:00-18:00
  - [ ] OK
  - [ ] LỖI
- 6.5 GridSplitter giữa lịch và panel → nhìn thấy + kéo được
  - [ ] OK
  - [ ] LỖI
- 6.6 Tạo cuộc họp 10 phút tới → chờ → Snackbar nhắc nhở hiện
  - [ ] OK
  - [ ] LỖI

📝 Ghi chú lỗi FB-6:

---

## 🟢 FB-7: Upload file cho AI

- 7.1 AI Kiểm tra VB → nút "📎 Tải file lên" → chọn .docx → nội dung hiện
  - [ ] OK
  - [ ] LỖI
- 7.2 AI Kiểm tra VB → nút "📎 Tải file mẫu đối chiếu" riêng
  - [ ] OK
  - [ ] LỖI
- 7.3 AI Tóm tắt → có nút upload file → hoạt động
  - [ ] OK
  - [ ] LỖI
- 7.4 AI Tham mưu → có nút upload file → hoạt động
  - [ ] OK
  - [ ] LỖI

📝 Ghi chú lỗi FB-7:

---

## 🟢 FB-8: 2 Options sau AI Kiểm tra VB

- 8.1 AI Kiểm tra → xong → nút "📄 Xuất Word" hiện → click → xuất OK
  - [ ] OK
  - [ ] LỖI
- 8.2 Nút "✏️ Soạn tiếp" → chuyển sang AI Soạn thảo với nội dung
  - [ ] OK
  - [ ] LỖI
- 8.3 Nút "✅ Áp dụng" → áp dụng vào VB gốc
  - [ ] OK
  - [ ] LỖI
- 8.4 Trước khi AI xong → 3 nút phải ẩn
  - [ ] OK
  - [ ] LỖI

📝 Ghi chú lỗi FB-8:

---

## 🟢 FB-9: Mẫu cuộc họp

- 9.1 Cuộc họp → menu ⋮ → "Lưu làm mẫu" → nhập tên → lưu OK
  - [ ] OK
  - [ ] LỖI
- 9.2 "Thêm ▼" → "Từ mẫu" → danh sách mẫu → chọn → form điền sẵn
  - [ ] OK
  - [ ] LỖI
- 9.3 Mẫu không hiện trong danh sách cuộc họp thường
  - [ ] OK
  - [ ] LỖI

📝 Ghi chú lỗi FB-9:

---

## 🟢 FB-10: Cập nhật pháp quy online

- 10.1 Tra cứu pháp quy → có nút "Kiểm tra cập nhật"
  - [ ] OK
  - [ ] LỖI
- 10.2 Bấm nút → hiện "Đã mới nhất" hoặc "Có bản mới"
  - [ ] OK
  - [ ] LỖI
- 10.3 Cuối trang hiện thời gian kiểm tra lần cuối
  - [ ] OK
  - [ ] LỖI

📝 Ghi chú lỗi FB-10:

---

## 📊 TỔNG KẾT (tự điền sau khi test xong)

- Tổng: 39 mục
- ✅ OK: ___
- ❌ Lỗi: ___
- ⬜ Chưa test: ___
- Ngày hoàn thành: ___/___/2025
