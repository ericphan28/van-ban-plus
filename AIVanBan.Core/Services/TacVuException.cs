namespace AIVanBan.Core.Services;

/// <summary>Mã lỗi tác vụ v2 (theo hợp đồng) cộng các lỗi phía máy: hết hạn chờ, mất kết nối.</summary>
public enum MaLoiTacVu
{
    /// <summary>402 KHONG_DU_CREDIT — chưa gọi AI, không trừ.</summary>
    KhongDuCredit,
    /// <summary>429 QUA_NHIEU_LUOT — đang có 3 lượt chạy cùng lúc, không trừ.</summary>
    QuaNhieuLuot,
    /// <summary>413 QUA_DAI — vượt giới hạn tác vụ hoặc dung lượng tệp của gói, không trừ.</summary>
    QuaDai,
    /// <summary>400 DAU_VAO_SAI — không trừ.</summary>
    DauVaoSai,
    /// <summary>503 AI_QUA_TAI + Retry-After — hoàn hết.</summary>
    AiQuaTai,
    /// <summary>502 AI_LOI — hoàn hết.</summary>
    AiLoi,
    /// <summary>499 NGAT_KET_NOI — hoàn hết.</summary>
    NgatKetNoi,
    /// <summary>401, hoặc máy chưa có khóa.</summary>
    ChuaDangNhap,
    /// <summary>404 — máy chủ chưa có tác vụ.</summary>
    KhongCoTacVu,
    /// <summary>500 và mọi phản hồi không đúng hợp đồng.</summary>
    LoiMayChu,
    /// <summary>Hết hạn chờ phía máy, hoặc 504 từ proxy.</summary>
    HetThoiGian,
    /// <summary>Không tới được máy chủ.</summary>
    MatKetNoi,
}

/// <summary>Lỗi khi gọi tác vụ v2. <see cref="Exception.Message"/> là lời hiện thẳng cho khách.</summary>
public sealed class TacVuException : Exception
{
    public TacVuException(MaLoiTacVu ma, string thongDiep, int? maHttp = null, TimeSpan? thuLaiSau = null,
        string? loiMayChu = null, Exception? goc = null)
        : base(thongDiep, goc)
    {
        Ma = ma;
        MaHttp = maHttp;
        ThuLaiSau = thuLaiSau;
        LoiMayChu = loiMayChu;
    }

    public MaLoiTacVu Ma { get; }

    /// <summary>Mã HTTP máy chủ trả; null khi lỗi xảy ra trước khi có phản hồi.</summary>
    public int? MaHttp { get; }

    /// <summary>Từ header Retry-After (AI_QUA_TAI).</summary>
    public TimeSpan? ThuLaiSau { get; }

    /// <summary>Trường message gốc của máy chủ (để ghi log / hỗ trợ).</summary>
    public string? LoiMayChu { get; }
}
