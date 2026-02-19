# 📄 Mẫu cấu trúc cho mỗi Văn bản Pháp quy

> Copy template này khi thêm văn bản mới vào hệ thống.

---

## README.md — Metadata văn bản

```markdown
# [Số hiệu văn bản]

## Thông tin chung

| Thuộc tính | Giá trị |
|-----------|---------|
| **Số hiệu** | [VD: 30/2020/NĐ-CP] |
| **Loại văn bản** | [Nghị định / Thông tư / Luật / Quyết định] |
| **Cơ quan ban hành** | [VD: Chính phủ] |
| **Người ký** | [VD: Thủ tướng Nguyễn Xuân Phúc] |
| **Ngày ban hành** | [VD: 05/03/2020] |
| **Ngày hiệu lực** | [VD: 05/03/2020] |
| **Tình trạng** | [Còn hiệu lực / Hết hiệu lực / Sửa đổi bổ sung] |
| **Lĩnh vực** | [VD: Hành chính, Văn thư] |
| **Văn bản thay thế** | [VD: Thay thế NĐ 110/2004/NĐ-CP] |

## Tóm tắt nội dung

[Mô tả ngắn gọn nội dung chính của văn bản]

## Cấu trúc

- Chương I: [Tên chương] (Điều 1-6)
- Chương II: [Tên chương] (Điều 7-15)
- ...
- Phụ lục I: [Tên phụ lục]
- Phụ lục II: [Tên phụ lục]

## Áp dụng trong VanBanPlus

- [ ] [Liệt kê tính năng nào trong app sử dụng văn bản này]

## Nguồn

- [thuvienphapluat.vn](link)
- [vanban.chinhphu.vn](link)
```

---

## noi-dung.md — Nội dung chính

```markdown
# [Số hiệu] — [Tên văn bản]

> Nguồn: [link gốc]
> Lưu ý: Chỉ trích dẫn các điều khoản liên quan đến VanBanPlus.
> Đánh dấu [RELEVANT] cho điều khoản áp dụng trực tiếp vào phần mềm.

## Chương I — QUY ĐỊNH CHUNG

### Điều 1. Phạm vi điều chỉnh

[Nội dung điều 1]

### Điều 2. Đối tượng áp dụng

[Nội dung điều 2]

...

## Chương II — ...

### Điều X. [Tên điều] [RELEVANT]

[Nội dung — đánh dấu RELEVANT nếu liên quan trực tiếp đến app]
```

---

## phu-luc/phu-luc-X.md — Phụ lục

```markdown
# Phụ lục [số] — [Tên phụ lục]

> Thuộc: [Số hiệu văn bản]
> Mục đích: [Mô tả phụ lục dùng để làm gì]
> Áp dụng trong app: [Tính năng nào sử dụng]

## Nội dung

[Nội dung phụ lục — có thể là bảng mẫu, danh sách, hướng dẫn]
```
