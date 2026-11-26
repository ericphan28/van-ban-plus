using AIVanBan.Core.Models;
using AIVanBan.Core.Services;

namespace AIVanBan.Tests;

/// <summary>
/// Quy tắc «quá hạn» dùng chung cho thông báo khi mở máy, dòng tô đỏ và nút lọc «Quá hạn».
/// Trước 1.0.23 nút lọc so với giờ hiện tại và không lọc hướng → con số lệch với thông báo.
/// </summary>
public class DocumentOverdueTests
{
    private static readonly DateTime Today = new(2026, 11, 26);

    private static Document Den(DateTime? due, DocumentStatus status = DocumentStatus.Draft) => new()
    {
        Direction = Direction.Den,
        DueDate = due,
        WorkflowStatus = status,
    };

    [Fact]
    public void HanHomQua_LaQuaHan() =>
        Assert.True(DocumentService.IsOverdue(Den(Today.AddDays(-1)), Today));

    [Fact]
    public void HanHomNay_ChuaQuaHan() =>
        Assert.False(DocumentService.IsOverdue(Den(Today), Today.AddHours(16)));

    [Fact]
    public void KhongCoHan_ChuaQuaHan() =>
        Assert.False(DocumentService.IsOverdue(Den(null), Today));

    [Fact]
    public void VanBanDi_CoHanDaQua_KhongTinh()
    {
        var d = Den(Today.AddDays(-5));
        d.Direction = Direction.Di;
        Assert.False(DocumentService.IsOverdue(d, Today));
    }

    [Theory]
    [InlineData(DocumentStatus.Archived)]
    [InlineData(DocumentStatus.Published)]
    public void DaLuuTruHoacBanHanh_KhongTinh(DocumentStatus status) =>
        Assert.False(DocumentService.IsOverdue(Den(Today.AddDays(-5), status), Today));

    [Fact]
    public void TrongThungRac_KhongTinh()
    {
        var d = Den(Today.AddDays(-5));
        d.IsDeleted = true;
        Assert.False(DocumentService.IsOverdue(d, Today));
    }
}
