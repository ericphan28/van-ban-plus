# Hướng dẫn tích hợp Google Gemini API — OCR & Trích xuất dữ liệu hóa đơn

> **Mục đích**: Document này dùng làm `copilot-instructions.md` (hoặc context file) cho GitHub Copilot trong project mới, giúp Copilot hiểu cách tích hợp Gemini API để OCR và trích xuất dữ liệu hóa đơn.
>
> **Nguồn tham khảo**: Project VanBanPlus — đã production với Gemini 2.5 Flash cho OCR văn bản hành chính.

---

## 1. Tổng quan kiến trúc Gemini API

### 1.1 Base URL & Endpoint

```
Base URL: https://generativelanguage.googleapis.com/v1beta/models
```

Hai endpoint chính:

| Endpoint | URL Pattern | Mô tả |
|----------|------------|-------|
| **generateContent** | `{BASE_URL}/{MODEL}:generateContent?key={API_KEY}` | Text generation + Vision (OCR) |
| **streamGenerateContent** | `{BASE_URL}/{MODEL}:streamGenerateContent?key={API_KEY}&alt=sse` | Streaming response (SSE) |

### 1.2 Model khuyến nghị

```
gemini-2.5-flash
```

- **Tốc độ nhanh**, chi phí thấp, hỗ trợ Vision (ảnh + PDF)
- Hỗ trợ **Structured Output** (JSON Schema) — đảm bảo 100% valid JSON
- Hỗ trợ **Thinking** (có thể tắt bằng `thinkingBudget: 0` để tiết kiệm token)
- Pricing (Paid Tier 1):
  - Input: $0.30 / 1M tokens
  - Output (incl. thinking): $2.50 / 1M tokens

---

## 2. Cấu hình & Authentication

### 2.1 API Key

Gemini dùng **API Key** đơn giản (không cần OAuth):

```
GET https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key=YOUR_API_KEY
```

**Lưu ý bảo mật:**
- **KHÔNG BAO GIỜ** để API Key trong client-side code
- Luôn giữ API Key ở server (backend/environment variable)
- Dùng proxy pattern: Client → Backend API → Gemini API

### 2.2 Cấu hình trong appsettings.json (.NET)

```json
{
  "Gemini": {
    "ApiKey": "YOUR_GEMINI_API_KEY_HERE",
    "Model": "gemini-2.5-flash"
  }
}
```

### 2.3 Cấu hình trong .env (Node.js/Python)

```env
GEMINI_API_KEY=YOUR_GEMINI_API_KEY_HERE
GEMINI_MODEL=gemini-2.5-flash
```

---

## 3. Request/Response Structure (JSON)

### 3.1 Request Body Schema

```json
{
  "contents": [
    {
      "parts": [
        { "text": "Prompt text ở đây" },
        {
          "inline_data": {
            "mime_type": "image/jpeg",
            "data": "BASE64_ENCODED_FILE_DATA"
          }
        }
      ]
    }
  ],
  "systemInstruction": {
    "parts": [
      { "text": "System instruction ở đây (optional)" }
    ]
  },
  "generationConfig": {
    "temperature": 0.1,
    "maxOutputTokens": 8192,
    "responseMimeType": "application/json",
    "responseSchema": { ... },
    "thinkingConfig": {
      "thinkingBudget": 0
    }
  }
}
```

### 3.2 Giải thích các field quan trọng

| Field | Mô tả | Giá trị khuyến nghị cho OCR |
|-------|--------|---------------------------|
| `contents[].parts[].text` | Prompt/instruction | Prompt trích xuất chi tiết |
| `contents[].parts[].inline_data` | File ảnh/PDF dạng base64 | Gửi ảnh hóa đơn |
| `inline_data.mime_type` | MIME type của file | `image/jpeg`, `image/png`, `application/pdf` |
| `inline_data.data` | Base64 encoded data | Không có prefix `data:...;base64,` |
| `systemInstruction` | System prompt (optional) | Mô tả vai trò AI |
| `temperature` | Độ sáng tạo (0.0–2.0) | **0.1** cho OCR (cần chính xác) |
| `maxOutputTokens` | Giới hạn output | 8192–65536 tùy độ dài hóa đơn |
| `responseMimeType` | Ép kiểu output | `"application/json"` cho structured output |
| `responseSchema` | JSON Schema ép cấu trúc | Định nghĩa schema hóa đơn |
| `thinkingConfig.thinkingBudget` | Budget cho thinking | **0** = tắt thinking (nhanh + rẻ hơn) |

### 3.3 Response Body Schema

```json
{
  "candidates": [
    {
      "content": {
        "parts": [
          { "text": "Nội dung response ở đây" }
        ]
      },
      "finishReason": "STOP"
    }
  ],
  "usageMetadata": {
    "promptTokenCount": 1234,
    "candidatesTokenCount": 567,
    "totalTokenCount": 1801
  }
}
```

**Lưu ý quan trọng:**
- Gemini 2.5 có thể trả **nhiều parts** (thinking + answer) → **luôn lấy part CUỐI CÙNG**
- `finishReason`: `"STOP"` = hoàn thành, `"MAX_TOKENS"` = bị cắt
- `usageMetadata` chứa thông tin token để tính chi phí

---

## 4. OCR Hóa đơn — Implementation Guide

### 4.1 MIME Types hỗ trợ

```
image/jpeg, image/png, image/gif, image/bmp, image/webp, image/tiff
application/pdf
```

### 4.2 Quy trình OCR hóa đơn

```
1. Nhận file ảnh/PDF hóa đơn từ user
2. Convert file → base64 string
3. Xác định MIME type từ extension
4. Gửi request tới Gemini với prompt trích xuất + inline_data
5. Parse JSON response → DTO hóa đơn
6. Validate & post-process dữ liệu
```

### 4.3 Prompt mẫu cho trích xuất hóa đơn

```text
Bạn là chuyên gia OCR và trích xuất dữ liệu hóa đơn.
Đọc file/ảnh hóa đơn này và trích xuất thông tin theo schema JSON đã khai báo.

QUY TẮC BẮT BUỘC:
1. Đọc TOÀN BỘ nội dung, giữ nguyên dấu tiếng Việt chính xác
2. Số tiền: giữ nguyên số, KHÔNG format lại
3. Ngày tháng: format dd/MM/yyyy
4. Mã số thuế: giữ nguyên (10 hoặc 13 chữ số)
5. Nếu field không tìm thấy → trả về chuỗi rỗng ""
6. Nếu có nhiều mặt hàng → trả về array đầy đủ
```

### 4.4 JSON Schema cho hóa đơn (Structured Output)

```json
{
  "type": "object",
  "properties": {
    "so_hoa_don": { "type": "string", "description": "Số hóa đơn" },
    "ky_hieu": { "type": "string", "description": "Ký hiệu hóa đơn" },
    "ngay_hoa_don": { "type": "string", "description": "Ngày hóa đơn dd/MM/yyyy" },
    "ma_so_thue_nguoi_ban": { "type": "string", "description": "MST người bán" },
    "ten_nguoi_ban": { "type": "string", "description": "Tên đơn vị bán" },
    "dia_chi_nguoi_ban": { "type": "string", "description": "Địa chỉ người bán" },
    "ma_so_thue_nguoi_mua": { "type": "string", "description": "MST người mua" },
    "ten_nguoi_mua": { "type": "string", "description": "Tên đơn vị mua" },
    "dia_chi_nguoi_mua": { "type": "string", "description": "Địa chỉ người mua" },
    "hinh_thuc_thanh_toan": { "type": "string", "description": "Hình thức thanh toán (TM/CK/TM-CK)" },
    "dong_tien": { "type": "string", "description": "Đồng tiền thanh toán (VND, USD...)" },
    "mat_hang": {
      "type": "array",
      "items": {
        "type": "object",
        "properties": {
          "stt": { "type": "integer" },
          "ten_hang": { "type": "string" },
          "don_vi_tinh": { "type": "string" },
          "so_luong": { "type": "number" },
          "don_gia": { "type": "number" },
          "thanh_tien": { "type": "number" },
          "thue_suat": { "type": "number", "description": "% thuế GTGT" }
        },
        "required": ["ten_hang", "thanh_tien"]
      },
      "description": "Danh sách hàng hóa/dịch vụ"
    },
    "tong_tien_truoc_thue": { "type": "number" },
    "tien_thue_gtgt": { "type": "number" },
    "tong_tien_thanh_toan": { "type": "number" },
    "so_tien_bang_chu": { "type": "string", "description": "Số tiền bằng chữ" },
    "ghi_chu": { "type": "string" }
  },
  "required": [
    "so_hoa_don", "ngay_hoa_don", "ten_nguoi_ban",
    "ma_so_thue_nguoi_ban", "mat_hang", "tong_tien_thanh_toan"
  ]
}
```

---

## 5. Code Implementation

### 5.1 C# (.NET) — Đầy đủ

```csharp
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

public class GeminiInvoiceService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;
    private const string API_BASE_URL = "https://generativelanguage.googleapis.com/v1beta/models";

    public GeminiInvoiceService(string apiKey, string model = "gemini-2.5-flash")
    {
        _apiKey = apiKey;
        _model = model;
        _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(120) };
    }

    /// <summary>
    /// Trích xuất dữ liệu hóa đơn từ file ảnh/PDF
    /// </summary>
    public async Task<InvoiceData> ExtractInvoiceAsync(string filePath)
    {
        var fileBytes = await File.ReadAllBytesAsync(filePath);
        var base64 = Convert.ToBase64String(fileBytes);
        var mimeType = GetMimeType(filePath);

        return await ExtractInvoiceFromBase64Async(base64, mimeType);
    }

    /// <summary>
    /// Trích xuất từ dữ liệu base64
    /// </summary>
    public async Task<InvoiceData> ExtractInvoiceFromBase64Async(string base64Data, string mimeType)
    {
        var prompt = @"Bạn là chuyên gia OCR và trích xuất dữ liệu hóa đơn Việt Nam.
Đọc file/ảnh hóa đơn này và trích xuất thông tin theo schema JSON đã khai báo.

QUY TẮC:
1. Đọc TOÀN BỘ nội dung, giữ nguyên dấu tiếng Việt
2. Số tiền: giữ nguyên số, KHÔNG format lại
3. Ngày tháng: format dd/MM/yyyy
4. MST: giữ nguyên (10 hoặc 13 chữ số)
5. Nếu field không tìm thấy → trả về chuỗi rỗng hoặc 0
6. Đọc TẤT CẢ các dòng mặt hàng";

        // JSON Schema ép cấu trúc output
        var invoiceSchema = new
        {
            type = "object",
            properties = new Dictionary<string, object>
            {
                ["so_hoa_don"] = new { type = "string" },
                ["ky_hieu"] = new { type = "string" },
                ["ngay_hoa_don"] = new { type = "string" },
                ["ma_so_thue_nguoi_ban"] = new { type = "string" },
                ["ten_nguoi_ban"] = new { type = "string" },
                ["dia_chi_nguoi_ban"] = new { type = "string" },
                ["ma_so_thue_nguoi_mua"] = new { type = "string" },
                ["ten_nguoi_mua"] = new { type = "string" },
                ["dia_chi_nguoi_mua"] = new { type = "string" },
                ["hinh_thuc_thanh_toan"] = new { type = "string" },
                ["mat_hang"] = new
                {
                    type = "array",
                    items = new
                    {
                        type = "object",
                        properties = new Dictionary<string, object>
                        {
                            ["stt"] = new { type = "integer" },
                            ["ten_hang"] = new { type = "string" },
                            ["don_vi_tinh"] = new { type = "string" },
                            ["so_luong"] = new { type = "number" },
                            ["don_gia"] = new { type = "number" },
                            ["thanh_tien"] = new { type = "number" },
                            ["thue_suat"] = new { type = "number" }
                        },
                        required = new[] { "ten_hang", "thanh_tien" }
                    }
                },
                ["tong_tien_truoc_thue"] = new { type = "number" },
                ["tien_thue_gtgt"] = new { type = "number" },
                ["tong_tien_thanh_toan"] = new { type = "number" },
                ["so_tien_bang_chu"] = new { type = "string" }
            },
            required = new[] { "so_hoa_don", "ngay_hoa_don", "ten_nguoi_ban",
                             "ma_so_thue_nguoi_ban", "mat_hang", "tong_tien_thanh_toan" }
        };

        var requestBody = new GeminiRequestBody
        {
            Contents = new[]
            {
                new GeminiContent
                {
                    Parts = new[]
                    {
                        new GeminiPart { Text = prompt },
                        new GeminiPart
                        {
                            InlineData = new GeminiInlineData
                            {
                                MimeType = mimeType,
                                Data = base64Data
                            }
                        }
                    }
                }
            },
            GenerationConfig = new GeminiGenerationConfig
            {
                Temperature = 0.1,        // Thấp = chính xác hơn
                MaxOutputTokens = 8192,
                ResponseMimeType = "application/json",  // Ép trả JSON
                ResponseSchema = invoiceSchema,          // Schema validation
                ThinkingConfig = new ThinkingConfig { ThinkingBudget = 0 } // Tắt thinking
            }
        };

        var url = $"{API_BASE_URL}/{_model}:generateContent?key={_apiKey}";

        var jsonOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        var json = JsonSerializer.Serialize(requestBody, jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(url, content);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<GeminiResponseBody>();

        // Lấy text từ part cuối cùng (Gemini 2.5 có thể trả nhiều parts)
        var parts = result?.Candidates?[0]?.Content?.Parts;
        var text = (parts != null && parts.Length > 0)
            ? parts[parts.Length - 1]?.Text ?? ""
            : "";

        return ParseInvoiceData(text);
    }

    private string GetMimeType(string filePath)
    {
        return Path.GetExtension(filePath).ToLower() switch
        {
            ".pdf" => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".webp" => "image/webp",
            ".tiff" or ".tif" => "image/tiff",
            _ => "application/octet-stream"
        };
    }

    private InvoiceData ParseInvoiceData(string jsonText)
    {
        // Clean markdown fences nếu có
        jsonText = jsonText.Trim();
        if (jsonText.StartsWith("```"))
        {
            var firstNewline = jsonText.IndexOf('\n');
            if (firstNewline > 0) jsonText = jsonText[(firstNewline + 1)..];
        }
        if (jsonText.EndsWith("```"))
            jsonText = jsonText[..^3];

        return JsonSerializer.Deserialize<InvoiceData>(jsonText.Trim()) ?? new InvoiceData();
    }

    // ============================================================
    // DTOs
    // ============================================================

    #region Gemini API DTOs

    private class GeminiRequestBody
    {
        [JsonPropertyName("contents")]
        public GeminiContent[] Contents { get; set; } = [];

        [JsonPropertyName("systemInstruction")]
        public GeminiContent? SystemInstruction { get; set; }

        [JsonPropertyName("generationConfig")]
        public GeminiGenerationConfig? GenerationConfig { get; set; }
    }

    private class GeminiContent
    {
        [JsonPropertyName("parts")]
        public GeminiPart[] Parts { get; set; } = [];
    }

    private class GeminiPart
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }

        [JsonPropertyName("inline_data")]
        public GeminiInlineData? InlineData { get; set; }
    }

    private class GeminiInlineData
    {
        [JsonPropertyName("mime_type")]
        public string MimeType { get; set; } = "";

        [JsonPropertyName("data")]
        public string Data { get; set; } = "";
    }

    private class GeminiGenerationConfig
    {
        [JsonPropertyName("temperature")]
        public double? Temperature { get; set; }

        [JsonPropertyName("maxOutputTokens")]
        public int? MaxOutputTokens { get; set; }

        [JsonPropertyName("responseMimeType")]
        public string? ResponseMimeType { get; set; }

        [JsonPropertyName("responseSchema")]
        public object? ResponseSchema { get; set; }

        [JsonPropertyName("thinkingConfig")]
        public ThinkingConfig? ThinkingConfig { get; set; }
    }

    private class ThinkingConfig
    {
        [JsonPropertyName("thinkingBudget")]
        public int ThinkingBudget { get; set; }
    }

    private class GeminiResponseBody
    {
        [JsonPropertyName("candidates")]
        public GeminiCandidate[]? Candidates { get; set; }

        [JsonPropertyName("usageMetadata")]
        public GeminiUsageMetadata? UsageMetadata { get; set; }
    }

    private class GeminiCandidate
    {
        [JsonPropertyName("content")]
        public GeminiContent? Content { get; set; }

        [JsonPropertyName("finishReason")]
        public string? FinishReason { get; set; }
    }

    private class GeminiUsageMetadata
    {
        [JsonPropertyName("promptTokenCount")]
        public int PromptTokenCount { get; set; }

        [JsonPropertyName("candidatesTokenCount")]
        public int CandidatesTokenCount { get; set; }

        [JsonPropertyName("totalTokenCount")]
        public int TotalTokenCount { get; set; }
    }

    #endregion
}

// DTO cho dữ liệu hóa đơn
public class InvoiceData
{
    [JsonPropertyName("so_hoa_don")]
    public string SoHoaDon { get; set; } = "";

    [JsonPropertyName("ky_hieu")]
    public string KyHieu { get; set; } = "";

    [JsonPropertyName("ngay_hoa_don")]
    public string NgayHoaDon { get; set; } = "";

    [JsonPropertyName("ma_so_thue_nguoi_ban")]
    public string MaSoThueNguoiBan { get; set; } = "";

    [JsonPropertyName("ten_nguoi_ban")]
    public string TenNguoiBan { get; set; } = "";

    [JsonPropertyName("dia_chi_nguoi_ban")]
    public string DiaChiNguoiBan { get; set; } = "";

    [JsonPropertyName("ma_so_thue_nguoi_mua")]
    public string MaSoThueNguoiMua { get; set; } = "";

    [JsonPropertyName("ten_nguoi_mua")]
    public string TenNguoiMua { get; set; } = "";

    [JsonPropertyName("dia_chi_nguoi_mua")]
    public string DiaChiNguoiMua { get; set; } = "";

    [JsonPropertyName("hinh_thuc_thanh_toan")]
    public string HinhThucThanhToan { get; set; } = "";

    [JsonPropertyName("mat_hang")]
    public InvoiceItem[] MatHang { get; set; } = [];

    [JsonPropertyName("tong_tien_truoc_thue")]
    public decimal TongTienTruocThue { get; set; }

    [JsonPropertyName("tien_thue_gtgt")]
    public decimal TienThueGtgt { get; set; }

    [JsonPropertyName("tong_tien_thanh_toan")]
    public decimal TongTienThanhToan { get; set; }

    [JsonPropertyName("so_tien_bang_chu")]
    public string SoTienBangChu { get; set; } = "";
}

public class InvoiceItem
{
    [JsonPropertyName("stt")]
    public int Stt { get; set; }

    [JsonPropertyName("ten_hang")]
    public string TenHang { get; set; } = "";

    [JsonPropertyName("don_vi_tinh")]
    public string DonViTinh { get; set; } = "";

    [JsonPropertyName("so_luong")]
    public decimal SoLuong { get; set; }

    [JsonPropertyName("don_gia")]
    public decimal DonGia { get; set; }

    [JsonPropertyName("thanh_tien")]
    public decimal ThanhTien { get; set; }

    [JsonPropertyName("thue_suat")]
    public decimal ThueSuat { get; set; }
}
```

### 5.2 TypeScript/Node.js

```typescript
const GEMINI_API_BASE = "https://generativelanguage.googleapis.com/v1beta/models";

interface GeminiRequest {
  contents: { parts: GeminiPart[] }[];
  systemInstruction?: { parts: { text: string }[] };
  generationConfig?: {
    temperature?: number;
    maxOutputTokens?: number;
    responseMimeType?: string;
    responseSchema?: object;
    thinkingConfig?: { thinkingBudget: number };
  };
}

interface GeminiPart {
  text?: string;
  inline_data?: { mime_type: string; data: string };
}

interface GeminiResponse {
  candidates?: {
    content?: { parts?: { text?: string }[] };
    finishReason?: string;
  }[];
  usageMetadata?: {
    promptTokenCount: number;
    candidatesTokenCount: number;
    totalTokenCount: number;
  };
}

async function extractInvoice(
  base64Data: string,
  mimeType: string,
  apiKey: string,
  model = "gemini-2.5-flash"
): Promise<InvoiceData> {
  const prompt = `Bạn là chuyên gia OCR và trích xuất dữ liệu hóa đơn Việt Nam.
Đọc file/ảnh hóa đơn này và trích xuất thông tin theo schema JSON đã khai báo.
Giữ nguyên dấu tiếng Việt. Ngày tháng format dd/MM/yyyy.`;

  const invoiceSchema = {
    type: "object",
    properties: {
      so_hoa_don: { type: "string" },
      ky_hieu: { type: "string" },
      ngay_hoa_don: { type: "string" },
      ma_so_thue_nguoi_ban: { type: "string" },
      ten_nguoi_ban: { type: "string" },
      dia_chi_nguoi_ban: { type: "string" },
      ma_so_thue_nguoi_mua: { type: "string" },
      ten_nguoi_mua: { type: "string" },
      dia_chi_nguoi_mua: { type: "string" },
      mat_hang: {
        type: "array",
        items: {
          type: "object",
          properties: {
            stt: { type: "integer" },
            ten_hang: { type: "string" },
            don_vi_tinh: { type: "string" },
            so_luong: { type: "number" },
            don_gia: { type: "number" },
            thanh_tien: { type: "number" },
            thue_suat: { type: "number" },
          },
          required: ["ten_hang", "thanh_tien"],
        },
      },
      tong_tien_truoc_thue: { type: "number" },
      tien_thue_gtgt: { type: "number" },
      tong_tien_thanh_toan: { type: "number" },
      so_tien_bang_chu: { type: "string" },
    },
    required: ["so_hoa_don", "ngay_hoa_don", "ten_nguoi_ban", "mat_hang", "tong_tien_thanh_toan"],
  };

  const body: GeminiRequest = {
    contents: [
      {
        parts: [
          { text: prompt },
          { inline_data: { mime_type: mimeType, data: base64Data } },
        ],
      },
    ],
    generationConfig: {
      temperature: 0.1,
      maxOutputTokens: 8192,
      responseMimeType: "application/json",
      responseSchema: invoiceSchema,
      thinkingConfig: { thinkingBudget: 0 },
    },
  };

  const url = `${GEMINI_API_BASE}/${model}:generateContent?key=${apiKey}`;
  const response = await fetch(url, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(body),
  });

  if (!response.ok) throw new Error(`Gemini API error: ${response.status}`);

  const result: GeminiResponse = await response.json();
  const parts = result.candidates?.[0]?.content?.parts;
  const text = parts && parts.length > 0 ? parts[parts.length - 1]?.text ?? "" : "";

  return JSON.parse(text) as InvoiceData;
}
```

### 5.3 Python

```python
import base64
import httpx  # hoặc requests
import json
from pathlib import Path

GEMINI_API_BASE = "https://generativelanguage.googleapis.com/v1beta/models"

async def extract_invoice(file_path: str, api_key: str, model: str = "gemini-2.5-flash") -> dict:
    """Trích xuất dữ liệu hóa đơn từ file ảnh/PDF bằng Gemini Vision"""

    # 1. Đọc file và convert base64
    file_bytes = Path(file_path).read_bytes()
    base64_data = base64.b64encode(file_bytes).decode("utf-8")

    # 2. Xác định MIME type
    ext = Path(file_path).suffix.lower()
    mime_map = {
        ".pdf": "application/pdf", ".jpg": "image/jpeg", ".jpeg": "image/jpeg",
        ".png": "image/png", ".gif": "image/gif", ".webp": "image/webp",
    }
    mime_type = mime_map.get(ext, "application/octet-stream")

    # 3. Tạo prompt
    prompt = """Bạn là chuyên gia OCR và trích xuất dữ liệu hóa đơn Việt Nam.
Đọc file/ảnh hóa đơn này và trích xuất thông tin theo schema JSON đã khai báo.
Giữ nguyên dấu tiếng Việt. Ngày tháng format dd/MM/yyyy."""

    # 4. JSON Schema cho structured output
    invoice_schema = {
        "type": "object",
        "properties": {
            "so_hoa_don": {"type": "string"},
            "ky_hieu": {"type": "string"},
            "ngay_hoa_don": {"type": "string"},
            "ma_so_thue_nguoi_ban": {"type": "string"},
            "ten_nguoi_ban": {"type": "string"},
            "dia_chi_nguoi_ban": {"type": "string"},
            "ma_so_thue_nguoi_mua": {"type": "string"},
            "ten_nguoi_mua": {"type": "string"},
            "dia_chi_nguoi_mua": {"type": "string"},
            "mat_hang": {
                "type": "array",
                "items": {
                    "type": "object",
                    "properties": {
                        "stt": {"type": "integer"},
                        "ten_hang": {"type": "string"},
                        "don_vi_tinh": {"type": "string"},
                        "so_luong": {"type": "number"},
                        "don_gia": {"type": "number"},
                        "thanh_tien": {"type": "number"},
                        "thue_suat": {"type": "number"},
                    },
                    "required": ["ten_hang", "thanh_tien"],
                },
            },
            "tong_tien_truoc_thue": {"type": "number"},
            "tien_thue_gtgt": {"type": "number"},
            "tong_tien_thanh_toan": {"type": "number"},
            "so_tien_bang_chu": {"type": "string"},
        },
        "required": ["so_hoa_don", "ngay_hoa_don", "ten_nguoi_ban", "mat_hang", "tong_tien_thanh_toan"],
    }

    # 5. Build request body
    body = {
        "contents": [{
            "parts": [
                {"text": prompt},
                {"inline_data": {"mime_type": mime_type, "data": base64_data}},
            ]
        }],
        "generationConfig": {
            "temperature": 0.1,
            "maxOutputTokens": 8192,
            "responseMimeType": "application/json",
            "responseSchema": invoice_schema,
            "thinkingConfig": {"thinkingBudget": 0},
        },
    }

    # 6. Gọi API
    url = f"{GEMINI_API_BASE}/{model}:generateContent?key={api_key}"
    async with httpx.AsyncClient(timeout=120) as client:
        response = await client.post(url, json=body)
        response.raise_for_status()

    # 7. Parse response — luôn lấy part cuối cùng
    result = response.json()
    parts = result.get("candidates", [{}])[0].get("content", {}).get("parts", [])
    text = parts[-1].get("text", "") if parts else ""

    return json.loads(text)
```

---

## 6. Kỹ thuật nâng cao (Production-ready)

### 6.1 Retry cho 429 Rate Limit

```csharp
private const int MAX_RETRIES = 3;
private static readonly int[] RETRY_WAIT_SECONDS = { 5, 10, 15 };

private async Task<HttpResponseMessage> SendWithRetryAsync(Func<Task<HttpResponseMessage>> sendFunc)
{
    HttpResponseMessage? response = null;
    for (int attempt = 0; attempt < MAX_RETRIES; attempt++)
    {
        response = await sendFunc();

        if ((int)response.StatusCode == 429 && attempt < MAX_RETRIES - 1)
        {
            var waitSec = RETRY_WAIT_SECONDS[attempt];
            await Task.Delay(waitSec * 1000);
            continue;
        }
        break;
    }
    return response!;
}
```

### 6.2 Multi-page OCR (nhiều ảnh = 1 hóa đơn)

Gemini hỗ trợ **nhiều `inline_data` parts** trong cùng 1 request:

```csharp
// Gửi nhiều trang cùng lúc
var parts = new List<GeminiPart>
{
    new GeminiPart { Text = $"Hóa đơn này gồm {files.Count} trang. Đọc tất cả và trích xuất." }
};

foreach (var file in files)
{
    parts.Add(new GeminiPart
    {
        InlineData = new GeminiInlineData
        {
            MimeType = file.MimeType,
            Data = file.Base64
        }
    });
}

var requestBody = new GeminiRequestBody
{
    Contents = new[] { new GeminiContent { Parts = parts.ToArray() } },
    GenerationConfig = new GeminiGenerationConfig
    {
        Temperature = 0.1,
        MaxOutputTokens = 16384,
        ResponseMimeType = "application/json",
        ResponseSchema = invoiceSchema,
        ThinkingConfig = new ThinkingConfig { ThinkingBudget = 0 }
    }
};
```

### 6.3 Streaming Response (Server-Sent Events)

```csharp
// URL streaming
var url = $"{API_BASE_URL}/gemini-2.5-flash:streamGenerateContent?key={apiKey}&alt=sse";

var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
var stream = await response.Content.ReadAsStreamAsync();
using var reader = new StreamReader(stream);

while (!reader.EndOfStream)
{
    var line = await reader.ReadLineAsync();
    if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data: ")) continue;

    var jsonData = line.Substring(6); // Remove "data: " prefix
    if (jsonData == "[DONE]") break;

    var chunk = JsonSerializer.Deserialize<GeminiResponse>(jsonData);
    var text = chunk?.Candidates?[0]?.Content?.Parts?[0]?.Text;
    if (!string.IsNullOrEmpty(text))
    {
        // Yield hoặc write to UI
        Console.Write(text);
    }
}
```

### 6.4 Fallback khi JSON parse lỗi

```csharp
/// Trích xuất field bằng regex khi JSON bị cắt/invalid
private string ExtractJsonField(string text, string fieldName)
{
    if (string.IsNullOrEmpty(text)) return "";
    var pattern = $@"""{fieldName}""\s*:\s*""((?:[^""\\]|\\.)*)""";
    var match = Regex.Match(text, pattern, RegexOptions.Singleline);
    if (match.Success && match.Groups.Count > 1)
    {
        return match.Groups[1].Value
            .Replace("\\n", "\n")
            .Replace("\\\"", "\"")
            .Replace("\\\\", "\\");
    }
    return "";
}
```

### 6.5 Ước tính chi phí

```csharp
// Gemini 2.5 Flash Paid Tier 1 (VNĐ, tỷ giá ~25,300 VNĐ/USD)
const decimal COST_PER_INPUT_TOKEN = 0.00759m;   // VNĐ per token
const decimal COST_PER_OUTPUT_TOKEN = 0.06325m;  // VNĐ per token

var cost = promptTokens * COST_PER_INPUT_TOKEN + completionTokens * COST_PER_OUTPUT_TOKEN;
```

---

## 7. Structured Output — Tính năng quan trọng nhất

### Tại sao dùng Structured Output?

- Gemini **đảm bảo 100% valid JSON** theo schema đã khai báo
- **Không cần** parse text rồi tìm JSON block
- **Không cần** xử lý markdown code fences
- Giảm hallucination — AI chỉ trả đúng fields đã định nghĩa

### Cách bật Structured Output:

```json
"generationConfig": {
    "responseMimeType": "application/json",
    "responseSchema": {
        "type": "object",
        "properties": { ... },
        "required": [ ... ]
    }
}
```

### Lưu ý:
- `responseMimeType` phải là `"application/json"`
- `responseSchema` theo chuẩn JSON Schema (subset)
- Hỗ trợ: `string`, `number`, `integer`, `boolean`, `array`, `object`
- Hỗ trợ `enum` để giới hạn giá trị
- **KHÔNG** hỗ trợ `$ref`, `oneOf`, `anyOf`, `allOf`, `additionalProperties`

---

## 8. Lưu ý & Best Practices

### ✅ NÊN LÀM:
1. **Temperature thấp (0.1)** cho OCR — cần chính xác, không sáng tạo
2. **Tắt thinking** (`thinkingBudget: 0`) cho OCR — tiết kiệm token + nhanh hơn
3. **Dùng Structured Output** — đảm bảo JSON valid, dễ parse
4. **Retry cho 429** — Rate limit phổ biến, đặc biệt free tier
5. **Timeout đủ dài** — 120s cho ảnh đơn, 300s cho PDF nhiều trang
6. **Lấy part cuối cùng** — Gemini 2.5 trả thinking + answer trong nhiều parts
7. **Xử lý null/empty** — Không phải lúc nào AI cũng tìm thấy tất cả fields

### ❌ KHÔNG NÊN:
1. **KHÔNG** để API Key trong frontend/client code
2. **KHÔNG** dùng temperature cao (>0.5) cho OCR
3. **KHÔNG** gửi file quá lớn (>20MB) — compress trước
4. **KHÔNG** tin tưởng 100% kết quả AI — luôn validate/review
5. **KHÔNG** gọi quá nhiều request cùng lúc (rate limit)

### ⚠️ Giới hạn:
- Free tier: 15 requests/phút, 1M tokens/phút
- Paid tier: 2000 requests/phút
- Max file size cho inline_data: ~20MB (base64)
- Max input tokens: 1M tokens (Gemini 2.5 Flash)

---

## 9. Tham khảo

- [Gemini API Documentation](https://ai.google.dev/gemini-api/docs)
- [Gemini Pricing](https://ai.google.dev/gemini-api/docs/pricing)
- [Structured Output](https://ai.google.dev/gemini-api/docs/structured-output)
- [Vision/Multimodal](https://ai.google.dev/gemini-api/docs/vision)
- Source code tham khảo: `AIVanBan.Core/Services/GeminiAIService.cs` và `AIVanBan.API/Services/GeminiProxyService.cs`
