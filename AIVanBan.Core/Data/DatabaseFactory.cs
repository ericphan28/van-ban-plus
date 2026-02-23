using LiteDB;

namespace AIVanBan.Core.Data;

/// <summary>
/// Singleton factory cho LiteDB — đảm bảo chỉ có 1 LiteDatabase instance
/// cho toàn bộ ứng dụng, tránh lỗi file lock khi nhiều service cùng truy cập.
/// </summary>
public static class DatabaseFactory
{
    private static LiteDatabase? _instance;
    private static readonly object _lock = new();
    private static string? _dbPath;

    /// <summary>
    /// Đường dẫn thư mục Data (Documents\AIVanBan\Data)
    /// </summary>
    public static string DataPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        "AIVanBan",
        "Data"
    );

    /// <summary>
    /// Lấy shared LiteDatabase instance (thread-safe singleton).
    /// Tự tạo thư mục nếu chưa có.
    /// </summary>
    public static LiteDatabase GetDatabase(string? customPath = null)
    {
        if (_instance != null) return _instance;

        lock (_lock)
        {
            if (_instance != null) return _instance;

            // Cấu hình BsonMapper trước khi tạo database
            LiteDbConfig.ConfigureGlobalMapper();

            var dataPath = customPath ?? DataPath;
            Directory.CreateDirectory(dataPath);

            _dbPath = Path.Combine(dataPath, "documents.db");
            _instance = new LiteDatabase($"Filename={_dbPath};Connection=Shared");

            Console.WriteLine($"✅ DatabaseFactory: Opened {_dbPath}");
            return _instance;
        }
    }

    /// <summary>
    /// Đóng và giải phóng database instance.
    /// Chỉ gọi khi app tắt hoàn toàn.
    /// </summary>
    public static void Shutdown()
    {
        lock (_lock)
        {
            if (_instance != null)
            {
                Console.WriteLine("🔒 DatabaseFactory: Shutting down...");
                _instance.Dispose();
                _instance = null;
            }
        }
    }
}
