using AIVanBan.API.Middleware;
using AIVanBan.API.Models.DTOs;
using AIVanBan.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace AIVanBan.API.Controllers;

/// <summary>
/// AI Controller — Proxy các request AI tới Gemini.
/// Tất cả endpoint cần API key + kiểm tra quota.
/// </summary>
[ApiController]
[Route("api/ai")]
public class AIController : ControllerBase
{
    private readonly GeminiProxyService _geminiService;

    public AIController(GeminiProxyService geminiService)
    {
        _geminiService = geminiService;
    }

    /// <summary>
    /// POST /api/ai/generate — Tạo nội dung AI từ prompt.
    /// </summary>
    [HttpPost("generate")]
    public async Task<IActionResult> Generate([FromBody] GenerateRequest request)
    {
        var user = HttpContext.GetApiUser();
        if (user == null) return Unauthorized();

        if (string.IsNullOrWhiteSpace(request.Prompt))
            return BadRequest(ApiResponse<GenerateResponse>.Fail("Prompt không được để trống."));

        var result = await _geminiService.GenerateAsync(request, user, HttpContext.GetClientIp());
        return result.Success ? Ok(result) : StatusCode(500, result);
    }

    /// <summary>
    /// POST /api/ai/extract — Trích xuất thông tin văn bản từ ảnh/PDF (OCR + AI).
    /// </summary>
    [HttpPost("extract")]
    public async Task<IActionResult> ExtractDocument([FromBody] ExtractDocumentRequest request)
    {
        var user = HttpContext.GetApiUser();
        if (user == null) return Unauthorized();

        if (string.IsNullOrWhiteSpace(request.Base64Data))
            return BadRequest(ApiResponse<GenerateResponse>.Fail("Base64Data không được để trống."));
        if (string.IsNullOrWhiteSpace(request.MimeType))
            return BadRequest(ApiResponse<GenerateResponse>.Fail("MimeType không được để trống."));

        var result = await _geminiService.ExtractDocumentAsync(request, user, HttpContext.GetClientIp());
        return result.Success ? Ok(result) : StatusCode(500, result);
    }

    /// <summary>
    /// POST /api/ai/read-text — Đọc text thuần từ file ảnh/PDF.
    /// </summary>
    [HttpPost("read-text")]
    public async Task<IActionResult> ReadText([FromBody] ReadTextRequest request)
    {
        var user = HttpContext.GetApiUser();
        if (user == null) return Unauthorized();

        if (string.IsNullOrWhiteSpace(request.Base64Data))
            return BadRequest(ApiResponse<GenerateResponse>.Fail("Base64Data không được để trống."));

        var result = await _geminiService.ReadTextAsync(request, user, HttpContext.GetClientIp());
        return result.Success ? Ok(result) : StatusCode(500, result);
    }
}
