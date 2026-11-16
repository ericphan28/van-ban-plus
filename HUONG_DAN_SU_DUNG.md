# 📋 HƯỚNG DẪN SỬ DỤNG — VanBanPlus (AI Văn Bản Hành Chính)

> **Phiên bản:** 1.0 · **Công ty:** Công ty TNHH Gia Kiệm Số ([giakiemso.com](https://giakiemso.com))  
> Ứng dụng quản lý văn bản hành chính thông minh với trí tuệ nhân tạo (AI)

---

## 📖 Mục lục

1. [Giới thiệu chung](#1-giới-thiệu-chung)
2. [Cài đặt & Khởi động](#2-cài-đặt--khởi-động)
3. [Trang chủ (Dashboard)](#3-trang-chủ-dashboard)
4. [Quản lý tài liệu](#4-quản-lý-tài-liệu)
5. [Sổ văn bản đi/đến](#5-sổ-văn-bản-điđến)
6. [AI Báo cáo định kỳ](#6-ai-báo-cáo-định-kỳ)
7. [Mẫu văn bản](#7-mẫu-văn-bản)
8. [Album ảnh](#8-album-ảnh)
9. [Cuộc họp](#9-cuộc-họp)
10. [Cấu hình Album](#10-cấu-hình-album)
11. [Sao lưu & Khôi phục](#11-sao-lưu--khôi-phục)
12. [Các tính năng AI](#12-các-tính-năng-ai)
13. [Xuất file Word](#13-xuất-file-word)
14. [Tài khoản & Cài đặt](#14-tài-khoản--cài-đặt)
15. [Cập nhật ứng dụng](#15-cập-nhật-ứng-dụng)

---

## 1. Giới thiệu chung

**VanBanPlus** là phần mềm quản lý văn bản hành chính dành cho các cơ quan nhà nước, tổ chức chính trị - xã hội tại Việt Nam. Ứng dụng tích hợp trí tuệ nhân tạo (AI) giúp tự động hóa việc soạn thảo, trích xuất, phân tích và quản lý văn bản.

### Đối tượng sử dụng
- Cán bộ, công chức xã/phường/thị trấn
- Nhân viên văn phòng cấp huyện, tỉnh
- Cán bộ các tổ chức đoàn thể (Hội Nông dân, Hội Phụ nữ, Đoàn Thanh niên, Hội Cựu chiến binh, Mặt trận Tổ quốc...)
- Nhân viên hành chính các cơ quan, doanh nghiệp

### Đặc điểm nổi bật
- ✅ Quản lý văn bản đi/đến/nội bộ theo đúng quy định
- ✅ AI soạn văn bản tự động từ mẫu sẵn có
- ✅ AI đọc & trích xuất nội dung từ ảnh chụp/scan PDF
- ✅ AI kiểm tra lỗi chính tả, văn phong, thể thức
- ✅ AI tham mưu xử lý văn bản đến
- ✅ Xuất Word chuẩn Thông tư 01/2011/TT-BNV
- ✅ Quản lý cuộc họp, album ảnh, sổ văn bản
- ✅ Sao lưu & khôi phục dữ liệu tự động
- ✅ Dữ liệu lưu trữ trên máy tính (không cần Internet để xem dữ liệu)

---

## 2. Cài đặt & Khởi động

### Yêu cầu hệ thống
- Windows 10 trở lên (64-bit)
- .NET 9.0 Runtime
- Kết nối Internet (cho tính năng AI)

### Khởi động lần đầu
1. Mở ứng dụng **VanBanPlus**
2. Hệ thống hiện **Thiết lập cơ quan**: chọn loại cơ quan của bạn (UBND xã, Sở, Trường học, Đoàn thể...)
3. Hệ thống tự động tạo **cấu trúc thư mục** phù hợp với nghiệp vụ cơ quan
4. Đăng nhập hoặc đăng ký tài khoản VanBanPlus để sử dụng tính năng AI

### Hỗ trợ hơn 70 loại cơ quan
UBND xã/huyện/tỉnh · HĐND · Đảng ủy · Mặt trận Tổ quốc · Hội Nông dân · Hội Phụ nữ · Đoàn Thanh niên · Hội Cựu chiến binh · Công đoàn · Các Sở ban ngành · Ban Đảng · Trường Mầm non/Tiểu học/THCS/THPT/Đại học · Trạm Y tế · Trung tâm Y tế · Bệnh viện · và nhiều loại khác.

---

## 3. Trang chủ (Dashboard)

Là màn hình đầu tiên khi mở ứng dụng, gồm:

- **Thanh bên trái (Sidebar):** Menu điều hướng đến tất cả tính năng. Có thể thu gọn (chỉ hiện biểu tượng) hoặc mở rộng (hiện đầy đủ tên).
- **Thống kê nhanh** ở sidebar: Tổng số văn bản, Văn bản tháng này, Văn bản năm nay.
- **2 nút tắt:**
  - 📄 **Thêm tài liệu** — mở trang quản lý tài liệu
  - 🤖 **Tạo văn bản AI** — mở công cụ soạn văn bản bằng AI

---

## 4. Quản lý tài liệu

> Đây là tính năng cốt lõi của ứng dụng.

### 4.1 Cây thư mục

Bên trái màn hình có **cây thư mục phân cấp**:
- 📥 **Văn bản đến** — văn bản nhận từ bên ngoài
- 📤 **Văn bản đi** — văn bản cơ quan ban hành
- 📋 **Văn bản nội bộ** — văn bản lưu hành nội bộ
- 📦 **Lưu trữ** — văn bản đã lưu trữ

Bạn có thể **tạo thư mục con** không giới hạn cấp, **đổi tên** hoặc **xóa** thư mục.

### 4.2 Thêm văn bản mới

Nhấn nút **"+ Thêm"** để tạo văn bản thủ công. Các trường thông tin:

| Trường | Mô tả |
|--------|-------|
| Số văn bản | Ví dụ: 123/QĐ-UBND |
| Trích yếu | Nội dung tóm tắt của văn bản |
| Loại văn bản | Chọn từ 24 loại: Luật, Nghị định, Thông tư, Quyết định, Công văn, Báo cáo, Tờ trình, Kế hoạch... |
| Ngày ban hành | Ngày ký văn bản |
| Cơ quan ban hành | Tên cơ quan phát hành |
| Nơi nhận | Danh sách nơi nhận |
| Hướng | Đi / Đến / Nội bộ |
| Lĩnh vực | Kinh tế, Văn hóa, Giáo dục, Tư pháp... |
| Tags | Nhãn phân loại tùy ý |
| Nội dung | Nội dung đầy đủ của văn bản |
| Căn cứ pháp lý | Các văn bản được viện dẫn |
| Trạng thái hiệu lực | Còn hiệu lực / Hết hiệu lực |

### 4.3 Import từ ảnh chụp / PDF scan (OCR)

Đây là tính năng mạnh nhất — **AI đọc ảnh chụp hoặc file PDF scan** và tự động trích xuất toàn bộ thông tin.

**Cách dùng:**
1. Nhấn nút **"📷 Scan Import"**
2. Chọn file ảnh (JPG, PNG, BMP, TIFF, WebP) hoặc PDF
3. AI tự động đọc và trích xuất **14 trường dữ liệu**: Số VB, Trích yếu, Loại VB, Ngày ban hành, Cơ quan, Người ký, Nội dung, Nơi nhận, Căn cứ pháp lý, Hướng VB, Lĩnh vực, Địa danh, Chức danh ký, Thẩm quyền ký
4. Bạn xem lại, chỉnh sửa nếu cần, rồi nhấn **Lưu**

> 💡 **Mẹo:** Chụp ảnh văn bản bằng điện thoại → gửi về máy tính → Import vào VanBanPlus. Không cần gõ lại thủ công!

### 4.4 File đính kèm

Mỗi văn bản có thể đính kèm nhiều file:
- 📄 Word, PDF, Excel, PowerPoint
- 🖼️ Ảnh (JPG, PNG...)
- 📜 PDF đã ký số (Signed PDF)

Bạn có thể đánh dấu **file chính** (primary) để phân biệt với file phụ.

### 4.5 Quy trình xử lý văn bản đi

Văn bản đi có **quy trình trạng thái** từng bước:

```
Nháp → Trình ký → Đã duyệt → Đã ký → Đã phát hành → Đã gửi → Lưu trữ
```

Bạn chuyển trạng thái văn bản theo từng bước xử lý thực tế.

### 4.6 Tìm kiếm & Lọc

- 🔍 Tìm theo từ khóa (tiêu đề, số VB, trích yếu, nội dung)
- Lọc theo **loại văn bản** (Công văn, Quyết định, Báo cáo...)
- Lọc theo **hướng** (Đi / Đến / Nội bộ)
- Lọc theo **thư mục**
- Lọc theo **khoảng ngày**
- Lọc theo **năm**

### 4.7 Thao tác hàng loạt

Chọn nhiều văn bản cùng lúc để:
- 🗑️ **Xóa hàng loạt**
- 📁 **Di chuyển** sang thư mục khác
- 📊 **Xuất Excel** — danh sách văn bản đã chọn
- 📝 **Xuất Word** — nội dung từng văn bản ra file Word riêng

---

## 5. Sổ văn bản đi/đến

Sổ đăng ký văn bản theo **kiểu truyền thống** — giống sổ giấy nhưng trên máy tính.

### 3 tab riêng biệt:
- **Văn bản đi** — sổ đăng ký VB do cơ quan ban hành
- **Văn bản đến** — sổ đăng ký VB nhận được
- **Văn bản nội bộ** — sổ đăng ký VB nội bộ

### Mỗi tab hiển thị bảng:

| STT | Số VB | Ngày | Trích yếu | Cơ quan |
|-----|-------|------|-----------|---------|
| 1   | 123/CV-UBND | 15/01/2026 | V/v báo cáo tình hình... | UBND xã ABC |

- Lọc theo **năm**, **loại VB**, **từ khóa**
- Hiển thị **thống kê**: Tổng / Đi / Đến / Nội bộ

---

## 6. AI Báo cáo định kỳ

AI giúp soạn **báo cáo định kỳ** từ số liệu thô — tiết kiệm hàng giờ soạn thảo.

### Cách sử dụng:
1. Chọn **kỳ báo cáo**: Tuần / Tháng / Quý / Năm
2. Chọn **lĩnh vực**: Kinh tế - Xã hội, Cải cách hành chính, Tài chính, Giáo dục...
3. Nhập **tên đơn vị**
4. Dán **số liệu thô** (bảng tổng hợp, con số, kết quả)
5. *(Tùy chọn)* Dán **báo cáo kỳ trước** để AI so sánh
6. Nhập **người ký** và **chức danh**
7. Nhấn **"Tạo báo cáo"**

### AI tự động tạo:
- **Phần I: Kết quả thực hiện** — trình bày số liệu, tính % tăng/giảm, so sánh kỳ trước
- **Phần II: Đánh giá chung** — ưu điểm, tồn tại, nguyên nhân
- **Phần III: Phương hướng, kiến nghị** — nhiệm vụ trọng tâm kỳ tới

> 📌 Báo cáo được viết bằng **văn phong hành chính chuẩn**, không dùng markdown.

---

## 7. Mẫu văn bản

### 7.1 Kho mẫu sẵn có

Ứng dụng có sẵn **20+ mẫu** theo Nghị định 30/2020/NĐ-CP:

- Công văn · Quyết định · Báo cáo · Tờ trình · Kế hoạch
- Thông báo · Nghị quyết · Chỉ thị · Hướng dẫn · Quy định
- Chương trình · Phương án · Đề án · Biên bản · Hợp đồng
- Quy chế · Giấy mời · Giấy giới thiệu · Giấy ủy quyền · Giấy nghỉ phép

### 7.2 Tạo văn bản từ mẫu (AI)

1. Chọn mẫu văn bản cần tạo
2. Nhập các trường bắt buộc (người ký, nơi nhận, nội dung chính...)
3. AI tự động soạn văn bản hoàn chỉnh
4. Bạn xem lại, chỉnh sửa → Lưu vào hệ thống

> 💡 Mỗi mẫu có sẵn **kịch bản mẫu** (Sample Scenarios) giúp bạn thử nhanh.

### 7.3 Quản lý mẫu

- **Thêm mẫu mới** — tự tạo mẫu riêng cho cơ quan
- **Sửa mẫu** — chỉnh nội dung, prompt AI, trường bắt buộc
- **Xóa mẫu** — xóa mẫu không cần dùng
- **Reset mẫu mặc định** — khôi phục 20+ mẫu gốc

### 7.4 Kho mẫu trực tuyến (Template Store)

- Tải thêm mẫu từ server
- Xem trạng thái: **Chưa tải** / **Đã có** / **Có cập nhật mới**
- Cập nhật mẫu khi có phiên bản mới

---

## 8. Album ảnh

Quản lý **ảnh hoạt động cơ quan** theo album — phục vụ lưu trữ, báo cáo bằng hình ảnh.

### 8.1 Quản lý album

- **Tạo album mới**: Tiêu đề, Mô tả, Tags
- **Sửa album**: Đổi tên, mô tả
- **Xóa album**: Xóa cả ảnh vật lý
- **3 chế độ xem**: Lưới (Grid) · Thẻ (Cards) · Danh sách (List)

### 8.2 Cây thư mục album

Bên trái có **cây thư mục** để phân loại album:
- 📁 Tất cả
- 📁 Chưa phân loại
- 📁 Sự kiện · Hội nghị · Giáo dục · Xây dựng · Văn hóa...

Bạn có thể tạo thư mục con, kéo thả album vào thư mục.

### 8.3 Quản lý ảnh trong album

- **Import ảnh**: Kéo thả file hoặc chọn từ máy tính
- **Xem ảnh** phóng to (full-size)
- **Sửa thông tin ảnh**: Mô tả, Tags, Sự kiện, Địa điểm, Người chụp, Người trong ảnh
- **Đặt ảnh bìa** (cover photo) cho album
- **Xóa ảnh**

### 8.4 Tạo album từ mẫu cấu trúc

Chọn từ cấu trúc đã thiết lập sẵn → hệ thống **tự động tạo nhiều album** theo danh mục nghiệp vụ cơ quan.

---

## 9. Cuộc họp

Quản lý **toàn bộ quy trình cuộc họp** — từ lên lịch đến xuất biên bản.

### 9.1 Thông tin cuộc họp

| Trường | Mô tả |
|--------|-------|
| Tên cuộc họp | Tiêu đề chính |
| Loại | 21 loại: Thường kỳ, Giao ban, Chuyên đề, Sơ kết, Tổng kết, Chi bộ, Đảng ủy, HĐND, Tiếp công dân... |
| Cấp | Đơn vị / Xã / Huyện / Tỉnh / Trung ương / Liên ngành |
| Hình thức | Trực tiếp / Trực tuyến / Kết hợp (hybrid) |
| Trạng thái | Đã lên lịch → Đang diễn ra → Đã kết thúc / Hoãn / Hủy |
| Thời gian | Ngày giờ bắt đầu — kết thúc |
| Địa điểm | Phòng họp, link Zoom/Teams... |
| Nội dung | Chương trình nghị sự |
| Kết luận | Nội dung kết luận cuộc họp |

### 9.2 Thành phần tham dự

Mỗi cuộc họp có danh sách người tham dự:
- **Vai trò**: Chủ trì · Thư ký · Báo cáo viên · Thành viên · Dự thính · Được mời
- **Trạng thái**: Đã mời · Xác nhận · Có mặt · Vắng · Vắng có phép · Ủy quyền

### 9.3 Nhiệm vụ từ cuộc họp

Giao việc ngay trong cuộc họp:
- Nội dung nhiệm vụ, Người được giao, Đơn vị, Hạn hoàn thành
- Mức ưu tiên: 1 (Thấp) → 5 (Rất cao)
- Trạng thái: Chưa thực hiện → Đang thực hiện → Hoàn thành / Quá hạn / Hủy
- ⏰ Hệ thống **tự động đánh dấu quá hạn** cho nhiệm vụ chưa hoàn thành

### 9.4 Tài liệu cuộc họp

Liên kết tài liệu với cuộc họp:
- Giấy mời · Chương trình · Tài liệu họp · Biên bản
- Thông báo kết luận · Nghị quyết · Quyết định · Công văn chỉ đạo

### 9.5 Xuất Word cuộc họp

4 loại văn bản xuất:
1. 📝 **Biên bản cuộc họp** — đầy đủ theo chuẩn hành chính VN
2. 📋 **Thông báo kết luận** cuộc họp
3. 📊 **Báo cáo tổng hợp** cuộc họp (dùng nội bộ)
4. 📑 **Tổng hợp nhiều cuộc họp** (dạng danh sách)

> 📌 Tất cả đều theo đúng format Thông tư 01/2011/TT-BNV.

### 9.6 Tìm kiếm & Lọc

- Tìm theo từ khóa
- Lọc theo loại cuộc họp, trạng thái, khoảng ngày
- Thống kê theo tháng/năm

---

## 10. Cấu hình Album

Thiết lập **cấu trúc album ảnh** theo nghiệp vụ cơ quan.

- Mẫu mặc định: **12 danh mục chính**, **70+ phân loại con**
  - Sự kiện · Hội nghị · Giáo dục · Xây dựng · Văn hóa · Thể thao · An ninh trật tự · Môi trường · Y tế · Nông nghiệp...
- Tự động tạo cây thư mục album
- Gợi ý Tags cho mỗi album
- Có thể đồng bộ cấu trúc từ server

---

## 11. Sao lưu & Khôi phục

Bảo vệ dữ liệu của bạn — không lo mất dữ liệu.

### 11.1 Sao lưu thủ công

1. Nhấn nút **"Sao lưu ngay"**
2. Chọn nơi lưu hoặc để mặc định (`Tài liệu/AIVanBan/Backups/`)
3. Hệ thống tạo file `.zip` chứa **database** + **toàn bộ ảnh album**

### 11.2 Sao lưu tự động

- Tự động sao lưu mỗi khi mở ứng dụng
- Giữ tối đa **10 bản** sao lưu gần nhất

### 11.3 Khôi phục dữ liệu

1. Nhấn **"Khôi phục"**
2. Chọn file backup `.zip`
3. Hệ thống giải nén và phục hồi toàn bộ dữ liệu

### 11.4 Quản lý backup

- Xem danh sách backup: Ngày tạo, Kích thước
- Xóa backup cũ không cần dùng
- Mở thư mục dữ liệu / backup trong File Explorer
- Hiển thị dung lượng: Database + Album ảnh riêng biệt

---

## 12. Các tính năng AI

VanBanPlus tích hợp **7 tính năng AI**, chạy qua máy chủ VanBanPlus và tính bằng credit. Trước mỗi tác vụ, phần mềm báo khoảng credit dự kiến và số dư hiện có; tác vụ lỗi được hoàn credit.

- **Credit gói tháng** (cấp mới mỗi tháng, không cộng dồn): Miễn phí 30 · Khởi đầu 400 · Chuyên nghiệp 1.100 · Doanh nghiệp 3.000.
- **Gói nạp** mua thêm khi cần, mỗi gói dùng trong 90 ngày: 49.000đ = 160 credit · 99.000đ = 350 credit · 199.000đ = 750 credit.
- Phần mềm không gọi thẳng nhà cung cấp AI và không dùng khóa AI riêng; toàn bộ AI đi qua máy chủ VanBanPlus.
- **Lưu để hỗ trợ** (công tắc ở trang tài khoản trên web) là tùy chọn, mặc định tắt. Khi bật, bản sao của tác vụ được mã hóa và giữ 7 ngày để hỗ trợ khi có sự cố; tắt công tắc thì bản đã lưu bị xóa ngay.

### 🤖 AI 1: Soạn văn bản tự động

- Chọn mẫu văn bản → Nhập thông tin cần thiết → AI soạn nội dung hoàn chỉnh
- Địa phương trong văn bản (xã/phường, cơ quan cấp trên, tỉnh) lấy từ **Cài đặt → 🏛️ Cơ quan**
- Có kịch bản mẫu để thử nhanh
- Tạo xong → tự động lưu vào hệ thống

### 📷 AI 2: OCR trích xuất từ ảnh/PDF

- Tải lên ảnh chụp hoặc PDF scan
- AI đọc và trích xuất **14 trường metadata** tự động
- Xử lý được cả văn bản mờ, chất lượng thấp
- Tự động format nội dung (tách Điều/Khoản/Chương)

### 📖 AI 3: Đọc nội dung text

- Đọc ảnh/PDF → trả về **text thuần** (không trích xuất metadata)
- Dùng khi chỉ cần lấy nội dung chữ

### 📋 AI 4: Tóm tắt văn bản

Phân tích văn bản theo **10 mục**:
1. Tóm tắt tổng quan
2. Loại văn bản
3. Cơ quan ban hành
4. Đối tượng áp dụng
5. Nội dung chính (key points)
6. Căn cứ pháp lý
7. Các mốc thời gian quan trọng
8. Số liệu cụ thể
9. Tác động, ảnh hưởng
10. Ghi chú đặc biệt

### ✅ AI 5: Kiểm tra / Soát lỗi văn bản

Kiểm tra **8 khía cạnh**:

| Khía cạnh | Mô tả |
|-----------|-------|
| Chính tả | Lỗi chính tả, typo |
| Văn phong | Có đúng văn phong hành chính không |
| Xung đột nội dung | Các đoạn mâu thuẫn nhau |
| Logic & cấu trúc | Bố cục hợp lý không |
| Thiếu thành phần | Thiếu căn cứ, nơi nhận... |
| Nội dung mơ hồ | Câu chữ không rõ ràng |
| Đề xuất cải thiện | Gợi ý viết tốt hơn |
| Thể thức theo NĐ 30/2020 | Đúng quy định về thể thức |

Mỗi lỗi được đánh dấu:
- 🔴 **Nghiêm trọng (Critical)** — cần sửa ngay
- 🟡 **Cảnh báo (Warning)** — nên sửa
- 🟢 **Gợi ý (Suggestion)** — tham khảo

### 🧠 AI 6: Tham mưu xử lý văn bản đến

AI đóng vai **chuyên viên tham mưu**, phân tích văn bản đến và đề xuất:

| Mục | Nội dung |
|-----|---------|
| Tóm tắt | Nội dung chính của văn bản |
| Mức khẩn | Thường / Khẩn / Thượng khẩn / Hỏa tốc |
| Việc cần làm | Từng bước cụ thể |
| Deadline | Thời hạn xử lý |
| Người xử lý | Lãnh đạo/chuyên viên phụ trách |
| Đơn vị phối hợp | Các bộ phận liên quan |
| Thẩm quyền ký | CT/PCT-KT/PCT-VX... |
| Cần trả lời? | Có/Không |
| Loại VB trả lời | Công văn, Báo cáo, Tờ trình... |
| Dự thảo phản hồi | Gợi ý nội dung trả lời |
| Căn cứ pháp lý | Văn bản liên quan |
| Cảnh báo rủi ro | Các vấn đề cần lưu ý |
| Mức ưu tiên | Thấp → Cao |

> 💡 AI hiểu rõ bộ máy UBND xã/phường: Chủ tịch, Phó CT-KT, Phó CT-VX, Văn phòng-Thống kê, Tư pháp, Địa chính, Tài chính, VH-XH, Quân sự, Công an.

### 📊 AI 7: Báo cáo định kỳ

*(Xem mục 6 ở trên)*

---

## 13. Xuất file Word

Tất cả văn bản xuất ra **file .docx chuẩn hành chính Việt Nam**:

### Định dạng theo Thông tư 01/2011/TT-BNV:
- **Font:** Times New Roman 14pt
- **Giãn dòng:** 1.3
- **Lề:** Trên 2cm, Dưới 1.5cm, Trái 2cm, Phải 1cm

### Nội dung tự động tạo:
- Quốc hiệu + Tiêu ngữ (CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM — Độc lập - Tự do - Hạnh phúc)
- Tên cơ quan ban hành
- Số văn bản + Ngày tháng
- Loại văn bản + Trích yếu
- Kính gửi (cho Công văn)
- Căn cứ pháp lý
- Nội dung chính
- Khối chữ ký

### Hỗ trợ đặc biệt cho:
- **Quyết định**: Dòng thẩm quyền, nhãn "QUYẾT ĐỊNH"
- **Nghị quyết**: Nhãn "NGHỊ QUYẾT", cấu trúc Điều/Khoản
- **Chỉ thị**: Nhãn "CHỈ THỊ"

---

## 14. Tài khoản & Cài đặt

### 14.1 Đăng nhập / Đăng ký

- **Đăng nhập:** Email + Mật khẩu → nhận API Key để dùng AI
- **Đăng ký:** Họ tên, Email, Mật khẩu, Số điện thoại, Tên cơ quan

### 14.2 Hồ sơ người dùng

- Xem thông tin: Tên, Email, Gói dịch vụ đang dùng

### 14.3 Cài đặt API (nâng cao)

Tính năng AI chỉ kết nối qua máy chủ VanBanPlus bằng mã kích hoạt của tài khoản (từ bản 1.0.20 không còn chế độ kết nối trực tiếp tới nhà cung cấp AI). Tab **🏛️ Cơ quan** khai thêm xã/phường, cơ quan cấp trên, tỉnh để AI soạn thảo đúng địa phương; để trống cả ba thì văn bản không kèm bối cảnh địa phương.

### 14.4 Thanh trạng thái (Status Bar)

Ở cuối màn hình hiển thị:
- ☁️ **Trạng thái API**: Đang kết nối / Chưa cấu hình
- 👤 **Thông tin user**: Tên + Gói dịch vụ
- 📊 **Mức dùng tháng này**: Số yêu cầu + tokens
- 🔗 Nút: Đăng nhập/Đăng xuất, Cài đặt

---

## 15. Cập nhật ứng dụng

- Ứng dụng **tự động kiểm tra cập nhật** mỗi khi khởi động
- Khi có phiên bản mới → thông báo cho bạn
- Bạn có thể chọn: **Cập nhật ngay** / **Nhắc lại sau** / **Bỏ qua phiên bản này**
- Kiểm tra thủ công: Nhấn nút **"Kiểm tra cập nhật"** trên thanh công cụ

---

## ❓ Câu hỏi thường gặp

### Dữ liệu lưu ở đâu?
Tất cả dữ liệu lưu trên máy tính của bạn tại: `Tài liệu/AIVanBan/`
- `Data/documents.db` — database
- `Photos/` — album ảnh

### Cần Internet không?
- **Không cần** để xem/quản lý dữ liệu đã có
- **Cần Internet** khi sử dụng tính năng AI (soạn VB, OCR, kiểm tra lỗi...)

### Mất dữ liệu thì sao?
Sử dụng tính năng **Sao lưu & Khôi phục** (mục 11). Nên sao lưu định kỳ và giữ bản backup ở ổ USB hoặc cloud.

### Hỗ trợ bao nhiêu loại văn bản?
**24 loại** theo Nghị định 30/2020/NĐ-CP về công tác văn thư.

### AI có chính xác 100% không?
AI hỗ trợ soạn thảo và trích xuất, nhưng bạn **luôn cần kiểm tra lại** trước khi sử dụng chính thức. AI là trợ lý, không thay thế con người.

---

> 📧 **Hỗ trợ:** Liên hệ Công ty TNHH Gia Kiệm Số — [giakiemso.com](https://giakiemso.com)
