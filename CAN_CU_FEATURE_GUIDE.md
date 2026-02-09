# Hướng dẫn sử dụng tính năng CĂN CỨ (Based On)

## Tổng quan

Dựa trên phân tích các văn bản mẫu thực tế (Luật, Nghị định, Quyết định, Thông báo), chúng tôi đã bổ sung phần **CĂN CỨ** - một phần bắt buộc và quan trọng trong văn bản hành chính nhà nước Việt Nam.

## Cấu trúc văn bản hành chính VN (theo Thông tư 01/2011/TT-BNV)

1. **Header**: Tên cơ quan cấp trên + Tên đơn vị
2. **Số văn bản**: Số: XXX/YYY-ZZZ
3. **Ngày tháng**: Ngày ... tháng ... năm ...
4. **Loại văn bản**: QUYẾT ĐỊNH, NGHỊ ĐỊNH, THÔNG BÁO, v.v.
5. **Tiêu đề**: Về việc ABC
6. **🆕 CĂN CỨ**: Liệt kê các căn cứ pháp lý
7. **Nội dung**: Nội dung chi tiết văn bản
8. **Nơi nhận/Chữ ký**: Chức danh, Họ tên

## Tính năng mới

### 1. Nhập Căn cứ pháp lý

Khi tạo/sửa văn bản, bạn sẽ thấy trường **"Căn cứ pháp lý"** mới:

```
┌────────────────────────────────────────────────┐
│ Căn cứ pháp lý (mỗi dòng 1 căn cứ)            │
│                                                │
│ Căn cứ Luật Tổ chức chính quyền địa phương... │
│ Căn cứ Nghị định số 66/NQ-CP...               │
│ Theo đề nghị của Trưởng phòng...              │
│                                                │
└────────────────────────────────────────────────┘
```

**Cách nhập:**
- Mỗi dòng = 1 căn cứ
- Bắt đầu bằng "Căn cứ" hoặc "Theo" (nếu chưa có, hệ thống sẽ tự thêm "Căn cứ" khi xuất Word)
- Kết thúc bằng dấu chấm phẩy (;)

**Ví dụ:**
```
Căn cứ Luật Tổ chức chính quyền địa phương ngày 19/6/2015;
Căn cứ Nghị định số 66/NQ-CP ngày 26/3/2025;
Theo đề nghị của Trưởng phòng Tài chính ngày 15/3/2025;
```

### 2. Xuất Word với Căn cứ

Khi xuất văn bản ra Word (đơn lẻ hoặc hàng loạt), phần **CĂN CỨ** sẽ được render theo đúng định dạng chuẩn:

```
                    QUYẾT ĐỊNH
                 Về việc ABC XYZ
                ----------------------

Căn cứ Luật Tổ chức chính quyền địa phương ngày 19/6/2015;
Căn cứ Nghị định số 66/NQ-CP ngày 26/3/2025;
Theo đề nghị của Trưởng phòng Tài chính ngày 15/3/2025;

    Nội dung văn bản bắt đầu từ đây...
```

**Định dạng:**
- Font: Times New Roman 14pt
- Căn lề: Trái (không thụt đầu dòng)
- Line spacing: 1.3 (theo chuẩn Thông tư 01/2011)
- Khoảng cách: 1 dòng trống trước và sau phần căn cứ

### 3. Bulk Export (Xuất hàng loạt)

Phần căn cứ cũng được bao gồm khi xuất nhiều văn bản vào một file Word:
- Mỗi văn bản có phần căn cứ riêng
- Ngắt trang giữa các văn bản
- Định dạng nhất quán

## Cấu trúc kỹ thuật

### 1. Model (Document.cs)

```csharp
public class Document
{
    // ... các field khác ...
    
    /// <summary>
    /// Căn cứ pháp lý - Mảng các căn cứ (mỗi phần tử = 1 dòng)
    /// </summary>
    public string[] BasedOn { get; set; } = Array.Empty<string>();
}
```

### 2. UI (DocumentEditDialog.xaml)

```xml
<!-- CĂN CỨ - Phần quan trọng trong văn bản hành chính VN -->
<TextBox x:Name="txtBasedOn" 
         materialDesign:HintAssist.Hint="Căn cứ pháp lý..."
         AcceptsReturn="True"
         TextWrapping="Wrap"
         MinHeight="100"/>
```

### 3. Code-behind (DocumentEditDialog.xaml.cs)

**Load:**
```csharp
txtBasedOn.Text = string.Join(Environment.NewLine, Document.BasedOn);
```

**Save:**
```csharp
Document.BasedOn = txtBasedOn.Text
    .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
    .Select(s => s.Trim())
    .Where(s => !string.IsNullOrWhiteSpace(s))
    .ToArray();
```

### 4. Word Export (WordExportService.cs)

```csharp
private void AddBasedOn(Body body, DocModel document)
{
    foreach (var basedOnItem in document.BasedOn)
    {
        // Đảm bảo text bắt đầu bằng "Căn cứ" hoặc "Theo"
        var text = basedOnItem.Trim();
        if (!text.StartsWith("Căn cứ") && !text.StartsWith("Theo"))
            text = "Căn cứ " + text;
        
        // Tạo paragraph với format Times 14pt, căn trái
        // ...
    }
}
```

## Tham khảo văn bản mẫu

Các văn bản mẫu từ thực tế đều có phần căn cứ:

### Ví dụ 1: Luật
```
QUỐC HỘI
---------
LUẬT
Sửa đổi, bổ sung một số điều của Luật ABC

Căn cứ Hiến pháp nước Cộng hòa xã hội chủ nghĩa Việt Nam;
Quốc hội ban hành Luật sửa đổi, bổ sung...
```

### Ví dụ 2: Quyết định
```
CHỦ TỊCH ỦY BAN NHÂN DÂN TỈNH NGHỆ AN
----------------
QUYẾT ĐỊNH
Về việc phê duyệt kế hoạch...

Căn cứ Luật Tổ chức chính quyền địa phương ngày 19/6/2015;
Căn cứ Nghị định số 66/NQ-CP...;
Theo đề nghị của Giám đốc Sở...;

QUYẾT ĐỊNH:
...
```

### Ví dụ 3: Nghị định
```
CHÍNH PHỦ
---------
NGHỊ ĐỊNH
Về quản lý, phát triển và sử dụng...

Căn cứ Luật Tổ chức Chính phủ...;
Căn cứ Luật Quản lý, sử dụng tài sản công...;
Theo đề nghị của Bộ trưởng Bộ...;

CHÍNH PHỦ NGHỊ ĐỊNH:
...
```

## Lợi ích

✅ **Tuân thủ chuẩn**: Đúng theo Thông tư 01/2011/TT-BNV của Bộ Nội vụ
✅ **Hoàn chỉnh**: Bao gồm đầy đủ các phần của văn bản hành chính
✅ **Dễ sử dụng**: UI thân thiện với tooltip hướng dẫn
✅ **Linh hoạt**: Hỗ trợ nhiều căn cứ, tự động format
✅ **Nhất quán**: Định dạng giống nhau giữa single export và bulk export

## Ghi chú

- Phần căn cứ là **BẮT BUỘC** trong hầu hết văn bản hành chính VN
- Nếu không nhập căn cứ, phần này sẽ không xuất hiện trong Word (tương tự trường Subject)
- Hệ thống tự động thêm "Căn cứ" vào đầu dòng nếu thiếu
- Hỗ trợ cả "Căn cứ" và "Theo" (thường dùng cho đề nghị của cơ quan cấp dưới)

---

**Ngày tạo**: 2025-01-24
**Phiên bản**: 1.0
**Tác giả**: GitHub Copilot + User
