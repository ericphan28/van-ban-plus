using System.IO.Compression;

namespace AIVanBan.Core.Services;

/// <summary>
/// Service sao lưu và khôi phục dữ liệu VanBanPlus.
/// Dữ liệu gồm: documents.db (LiteDB) + thư mục Attachments + thư mục Photos (album ảnh).
/// </summary>
public class BackupService
{
    private readonly string _basePath;
    private readonly string _dataPath;
    private readonly string _photosPath;
    private readonly string _backupPath;
    private const int MaxAutoBackups = 10;

    public BackupService()
    {
        _basePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "AIVanBan");

        _dataPath = Path.Combine(_basePath, "Data");
        _photosPath = Path.Combine(_basePath, "Photos");
        _backupPath = Path.Combine(_basePath, "Backups");

        Directory.CreateDirectory(_backupPath);
    }

    /// <summary>
    /// Đường dẫn thư mục dữ liệu.
    /// </summary>
    public string DataPath => _dataPath;

    /// <summary>
    /// Đường dẫn thư mục backup mặc định.
    /// </summary>
    public string BackupPath => _backupPath;

    /// <summary>
    /// Sao lưu dữ liệu ra file .zip tại đường dẫn chỉ định.
    /// </summary>
    public BackupResult Backup(string? outputPath = null)
    {
        try
        {
            if (!Directory.Exists(_dataPath) && !Directory.Exists(_photosPath))
                return BackupResult.Fail("Không tìm thấy thư mục dữ liệu.");

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var fileName = $"VanBanPlus_Backup_{timestamp}.zip";
            var targetDir = outputPath ?? _backupPath;
            Directory.CreateDirectory(targetDir);
            var fullPath = Path.Combine(targetDir, fileName);

            if (File.Exists(fullPath))
                File.Delete(fullPath);

            // Tạo zip chứa cả Data/ và Photos/
            using (var archive = ZipFile.Open(fullPath, ZipArchiveMode.Create))
            {
                // Thêm thư mục Data/
                if (Directory.Exists(_dataPath))
                    AddDirectoryToZip(archive, _dataPath, "Data");

                // Thêm thư mục Photos/
                if (Directory.Exists(_photosPath))
                    AddDirectoryToZip(archive, _photosPath, "Photos");
            }

            var fileInfo = new FileInfo(fullPath);
            return BackupResult.Ok(fullPath, fileInfo.Length);
        }
        catch (Exception ex)
        {
            return BackupResult.Fail($"Lỗi sao lưu: {ex.Message}");
        }
    }

    /// <summary>
    /// Thêm 1 thư mục vào file zip (giữ cấu trúc thư mục).
    /// </summary>
    private void AddDirectoryToZip(ZipArchive archive, string sourceDir, string entryPrefix)
    {
        foreach (var filePath in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(sourceDir, filePath);
            var entryName = Path.Combine(entryPrefix, relativePath).Replace('\\', '/');
            archive.CreateEntryFromFile(filePath, entryName, CompressionLevel.Optimal);
        }
    }

    /// <summary>
    /// Khôi phục dữ liệu từ file .zip backup.
    /// </summary>
    public RestoreResult Restore(string zipFilePath)
    {
        try
        {
            if (!File.Exists(zipFilePath))
                return RestoreResult.Fail("File backup không tồn tại.");

            if (Path.GetExtension(zipFilePath).ToLower() != ".zip")
                return RestoreResult.Fail("File backup phải là file .zip");

            // Kiểm tra file zip có hợp lệ (có thư mục Data/ hoặc file .db)
            using (var archive = ZipFile.OpenRead(zipFilePath))
            {
                var hasData = archive.Entries.Any(e => 
                    e.FullName.StartsWith("Data/", StringComparison.OrdinalIgnoreCase) ||
                    e.Name.EndsWith(".db", StringComparison.OrdinalIgnoreCase));
                if (!hasData)
                    return RestoreResult.Fail("File backup không hợp lệ - không tìm thấy dữ liệu.");
            }

            // Backup hiện tại trước khi khôi phục (an toàn)
            var safetyBackup = Backup();

            // Kiểm tra cấu trúc file zip: có thư mục Data/ và Photos/ không?
            bool hasSubfolders;
            using (var archive = ZipFile.OpenRead(zipFilePath))
            {
                hasSubfolders = archive.Entries.Any(e => 
                    e.FullName.StartsWith("Data/", StringComparison.OrdinalIgnoreCase) ||
                    e.FullName.StartsWith("Photos/", StringComparison.OrdinalIgnoreCase));
            }

            if (hasSubfolders)
            {
                // Format mới: zip chứa Data/ và Photos/
                // Xóa dữ liệu cũ trong Data/
                ClearDirectory(_dataPath);

                // Xóa dữ liệu cũ trong Photos/
                ClearDirectory(_photosPath);

                // Giải nén vào thư mục gốc AIVanBan/
                ZipFile.ExtractToDirectory(zipFilePath, _basePath, true);
            }
            else
            {
                // Format cũ: zip chỉ chứa nội dung Data/ (không có prefix)
                ClearDirectory(_dataPath);
                Directory.CreateDirectory(_dataPath);
                ZipFile.ExtractToDirectory(zipFilePath, _dataPath, true);
            }

            return RestoreResult.Ok(safetyBackup.FilePath ?? "");
        }
        catch (Exception ex)
        {
            return RestoreResult.Fail($"Lỗi khôi phục: {ex.Message}");
        }
    }

    /// <summary>
    /// Xóa nội dung thư mục nhưng giữ lại thư mục.
    /// </summary>
    private void ClearDirectory(string path)
    {
        if (!Directory.Exists(path)) return;
        foreach (var file in Directory.GetFiles(path))
            File.Delete(file);
        foreach (var dir in Directory.GetDirectories(path))
            Directory.Delete(dir, true);
    }

    /// <summary>
    /// Sao lưu tự động khi mở app (giữ tối đa MaxAutoBackups bản).
    /// </summary>
    public BackupResult AutoBackup()
    {
        try
        {
            var autoDir = Path.Combine(_backupPath, "Auto");
            Directory.CreateDirectory(autoDir);

            // Kiểm tra backup gần nhất - nếu < 1 giờ thì bỏ qua
            var existing = GetBackupList(autoDir);
            if (existing.Any())
            {
                var latest = existing.First();
                if ((DateTime.Now - latest.CreatedDate).TotalHours < 1)
                    return BackupResult.Ok(latest.FilePath, latest.FileSize, skipped: true);
            }

            // Thực hiện backup
            var result = Backup(autoDir);

            // Xóa các bản cũ nếu vượt quá MaxAutoBackups
            if (result.Success)
                CleanupOldBackups(autoDir, MaxAutoBackups);

            return result;
        }
        catch (Exception ex)
        {
            return BackupResult.Fail($"Lỗi backup tự động: {ex.Message}");
        }
    }

    /// <summary>
    /// Lấy danh sách file backup, sắp xếp mới nhất trước.
    /// </summary>
    public List<BackupInfo> GetBackupList(string? folderPath = null)
    {
        var results = new List<BackupInfo>();
        var searchPaths = new List<string>();

        if (folderPath != null)
        {
            searchPaths.Add(folderPath);
        }
        else
        {
            searchPaths.Add(_backupPath);
            var autoDir = Path.Combine(_backupPath, "Auto");
            if (Directory.Exists(autoDir))
                searchPaths.Add(autoDir);
        }

        foreach (var dir in searchPaths)
        {
            if (!Directory.Exists(dir)) continue;

            foreach (var file in Directory.GetFiles(dir, "VanBanPlus_Backup_*.zip"))
            {
                var fi = new FileInfo(file);
                var isAuto = dir.Contains("Auto");
                results.Add(new BackupInfo
                {
                    FilePath = file,
                    FileName = fi.Name,
                    FileSize = fi.Length,
                    CreatedDate = fi.CreationTime,
                    IsAutoBackup = isAuto
                });
            }
        }

        return results.OrderByDescending(b => b.CreatedDate).ToList();
    }

    /// <summary>
    /// Xóa 1 file backup.
    /// </summary>
    public bool DeleteBackup(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return true;
            }
            return false;
        }
        catch { return false; }
    }

    /// <summary>
    /// Tính dung lượng toàn bộ dữ liệu (Data + Photos).
    /// </summary>
    public long GetDataSize()
    {
        long total = 0;

        if (Directory.Exists(_dataPath))
            total += Directory.GetFiles(_dataPath, "*", SearchOption.AllDirectories)
                .Sum(f => new FileInfo(f).Length);

        if (Directory.Exists(_photosPath))
            total += Directory.GetFiles(_photosPath, "*", SearchOption.AllDirectories)
                .Sum(f => new FileInfo(f).Length);

        return total;
    }

    /// <summary>
    /// Tính dung lượng riêng từng phần.
    /// </summary>
    public (long dataSize, long photosSize) GetDataSizeDetails()
    {
        long dataSize = 0;
        long photosSize = 0;

        if (Directory.Exists(_dataPath))
            dataSize = Directory.GetFiles(_dataPath, "*", SearchOption.AllDirectories)
                .Sum(f => new FileInfo(f).Length);

        if (Directory.Exists(_photosPath))
            photosSize = Directory.GetFiles(_photosPath, "*", SearchOption.AllDirectories)
                .Sum(f => new FileInfo(f).Length);

        return (dataSize, photosSize);
    }

    /// <summary>
    /// Xóa các bản backup cũ, giữ lại số lượng tối đa.
    /// </summary>
    private void CleanupOldBackups(string folder, int keepCount)
    {
        var files = Directory.GetFiles(folder, "VanBanPlus_Backup_*.zip")
            .Select(f => new FileInfo(f))
            .OrderByDescending(f => f.CreationTime)
            .Skip(keepCount)
            .ToList();

        foreach (var file in files)
        {
            try { file.Delete(); } catch { }
        }
    }

    /// <summary>
    /// Format dung lượng file.
    /// </summary>
    public static string FormatFileSize(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
        if (bytes < 1024 * 1024 * 1024) return $"{bytes / (1024.0 * 1024):F1} MB";
        return $"{bytes / (1024.0 * 1024 * 1024):F2} GB";
    }
}

#region Result Models

public class BackupResult
{
    public bool Success { get; set; }
    public bool Skipped { get; set; }
    public string? FilePath { get; set; }
    public long FileSize { get; set; }
    public string? ErrorMessage { get; set; }

    public static BackupResult Ok(string path, long size, bool skipped = false) =>
        new() { Success = true, FilePath = path, FileSize = size, Skipped = skipped };

    public static BackupResult Fail(string error) =>
        new() { Success = false, ErrorMessage = error };
}

public class RestoreResult
{
    public bool Success { get; set; }
    public string? SafetyBackupPath { get; set; }
    public string? ErrorMessage { get; set; }

    public static RestoreResult Ok(string safetyBackupPath) =>
        new() { Success = true, SafetyBackupPath = safetyBackupPath };

    public static RestoreResult Fail(string error) =>
        new() { Success = false, ErrorMessage = error };
}

public class BackupInfo
{
    public string FilePath { get; set; } = "";
    public string FileName { get; set; } = "";
    public long FileSize { get; set; }
    public DateTime CreatedDate { get; set; }
    public bool IsAutoBackup { get; set; }

    public string FileSizeFormatted => BackupService.FormatFileSize(FileSize);
    public string TypeLabel => IsAutoBackup ? "Tự động" : "Thủ công";
}

#endregion
