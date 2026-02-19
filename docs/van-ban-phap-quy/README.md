# 📚 Hệ thống Văn bản Pháp quy — VanBanPlus

> **Mục đích**: Lưu trữ nội dung các văn bản pháp quy nhà nước dưới dạng Markdown
> để GitHub Copilot có thể đọc, hiểu và áp dụng đúng nghiệp vụ vào phần mềm.

## 📋 Danh sách văn bản áp dụng

| STT | Số hiệu | Tên văn bản | Lĩnh vực | Trạng thái |
|-----|---------|-------------|----------|------------|
| 1 | 30/2020/NĐ-CP | Nghị định về công tác văn thư | Văn thư | ⏳ Chờ bổ sung |
| 2 | 01/2011/TT-BNV | Thông tư hướng dẫn thể thức và kỹ thuật trình bày văn bản hành chính | Thể thức VB | ⏳ Chờ bổ sung |
| 3 | 01/2019/TT-BNV | Thông tư quy định quy trình trao đổi, lưu trữ, xử lý tài liệu điện tử | Tài liệu điện tử | ⏳ Chờ bổ sung |
| 4 | Luật số 01/2011/QH13 | Luật Lưu trữ | Lưu trữ | ⏳ Chờ bổ sung |
| 5 | Luật số 16/2023/QH15 | Luật Lưu trữ (sửa đổi 2024) | Lưu trữ | ⏳ Chờ bổ sung |
| 6 | 09/2010/TT-BNV | Thông tư quy định về lập hồ sơ, nộp lưu, bảo quản hồ sơ | Hồ sơ | ⏳ Chờ bổ sung |
| 7 | 27/2016/TT-BNV | Thông tư quy định kỹ thuật bảo quản tài liệu giấy | Bảo quản | ⏳ Chờ bổ sung |

> **Quy ước trạng thái**: ✅ Đã có nội dung | ⏳ Chờ bổ sung | 🔄 Đang cập nhật

## 🗂️ Cấu trúc thư mục

```
docs/van-ban-phap-quy/
│
├── README.md                    ← File này (index tổng)
├── _MAPPING.md                  ← Ánh xạ: điều khoản → tính năng phần mềm
├── _TEMPLATE.md                 ← Mẫu cấu trúc cho mỗi văn bản
│
├── nghi-dinh/
│   └── 30-2020-ND-CP/
│       ├── README.md            ← Metadata + tóm tắt
│       ├── noi-dung.md          ← Nội dung chính (các chương/điều)
│       └── phu-luc/
│           ├── phu-luc-I.md     ← Mẫu trình bày thành phần thể thức
│           ├── phu-luc-II.md    ← Bản sao y...
│           ├── phu-luc-III.md
│           ├── phu-luc-IV.md
│           ├── phu-luc-V.md
│           └── phu-luc-VI.md
│
├── thong-tu/
│   ├── 01-2011-TT-BNV/
│   │   ├── README.md
│   │   ├── noi-dung.md
│   │   └── phu-luc/
│   ├── 01-2019-TT-BNV/
│   │   ├── README.md
│   │   └── noi-dung.md
│   └── 09-2010-TT-BNV/
│       ├── README.md
│       └── noi-dung.md
│
└── luat/
    ├── luat-luu-tru-2011/
    │   ├── README.md
    │   └── noi-dung.md
    └── luat-luu-tru-2024/
        ├── README.md
        └── noi-dung.md
```

## 🎯 Cách Copilot sử dụng

1. **Khi soạn template văn bản** → đọc `phu-luc/` để lấy mẫu trình bày chuẩn
2. **Khi validate thể thức** → đọc `noi-dung.md` để kiểm tra quy định
3. **Khi phát triển tính năng mới** → đọc `_MAPPING.md` để biết điều khoản nào áp dụng
4. **Khi AI soạn thảo văn bản** → đọc phụ lục mẫu để đảm bảo đúng format

## 📝 Quy tắc đóng góp nội dung

1. **Chỉ dùng Markdown** — không dùng PDF/Word (Copilot không đọc được)
2. **Giữ nguyên số hiệu điều khoản** — để dễ tra cứu và ánh xạ
3. **Ghi rõ nguồn** — link đến bản gốc trên thuvienphapluat.vn hoặc vanban.chinhphu.vn
4. **Đánh dấu [TODO]** — cho phần chưa có nội dung
5. **Mỗi phụ lục 1 file riêng** — dễ quản lý, dễ tham chiếu
