using LiteDB;
using AIVanBan.Core.Models;

namespace AIVanBan.Core.Data;

/// <summary>
/// Cấu hình BsonMapper toàn cục cho LiteDB.
/// Xử lý enum deserialization an toàn — nếu DB chứa giá trị enum 
/// mà version hiện tại không có, sẽ fallback thay vì crash.
/// 
/// GỌI ConfigureGlobalMapper() MỘT LẦN trước khi tạo bất kỳ LiteDatabase nào.
/// </summary>
public static class LiteDbConfig
{
    private static bool _configured = false;
    private static readonly object _lock = new();

    /// <summary>
    /// Đăng ký custom serializer cho tất cả enum types.
    /// An toàn khi gọi nhiều lần (chỉ chạy 1 lần thực sự).
    /// </summary>
    public static void ConfigureGlobalMapper()
    {
        if (_configured) return;
        
        lock (_lock)
        {
            if (_configured) return;
            
            var mapper = BsonMapper.Global;
            
            // === Document enums ===
            RegisterSafeEnum(mapper, DocumentType.Khac);
            RegisterSafeEnum(mapper, Direction.Den);
            RegisterSafeEnum(mapper, DocumentStatus.Draft);
            RegisterSafeEnum(mapper, UrgencyLevel.Thuong);
            RegisterSafeEnum(mapper, SecurityLevel.Thuong);
            RegisterSafeEnum(mapper, CopyType.SaoY);
            
            // === Organization / Attachment enums ===
            RegisterSafeEnum(mapper, OrganizationType.UbndXa);
            RegisterSafeEnum(mapper, AttachmentType.Other);
            
            // === Meeting enums ===
            RegisterSafeEnum(mapper, MeetingType.Khac);
            RegisterSafeEnum(mapper, MeetingStatus.Scheduled);
            RegisterSafeEnum(mapper, MeetingFormat.TrucTiep);
            RegisterSafeEnum(mapper, MeetingLevel.CapDonVi);
            RegisterSafeEnum(mapper, AttendeeRole.Attendee);
            RegisterSafeEnum(mapper, AttendanceStatus.Invited);
            RegisterSafeEnum(mapper, MeetingTaskStatus.NotStarted);
            RegisterSafeEnum(mapper, MeetingDocumentType.TaiLieuHop);

            _configured = true;
            Console.WriteLine("✅ LiteDB BsonMapper configured with safe enum deserialization");
        }
    }

    /// <summary>
    /// Helper: Đăng ký safe enum deserializer cho bất kỳ enum type nào.
    /// Nếu giá trị trong DB không parse được → trả về fallback.
    /// </summary>
    private static void RegisterSafeEnum<T>(BsonMapper mapper, T fallback) where T : struct, Enum
    {
        mapper.RegisterType<T>(
            serialize: val => val.ToString(),
            deserialize: bson =>
            {
                if (bson.IsString && Enum.TryParse<T>(bson.AsString, true, out var result))
                    return result;
                if (bson.IsInt32 && Enum.IsDefined(typeof(T), bson.AsInt32))
                    return (T)(object)bson.AsInt32;
                Console.WriteLine($"⚠️ LiteDB: Unknown {typeof(T).Name} '{bson}' → fallback {fallback}");
                return fallback;
            }
        );
    }
}
