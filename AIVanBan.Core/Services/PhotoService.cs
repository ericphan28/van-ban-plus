using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Drawing;
using System.Drawing.Imaging;
using AIVanBan.Core.Models;

namespace AIVanBan.Core.Services;

/// <summary>
/// Service quản lý ảnh: Upload, Thumbnail, Metadata
/// </summary>
public class PhotoService : IDisposable
{
    private readonly string _photosBasePath;
    private readonly string _thumbnailCachePath;
    private readonly AlbumStructureService _albumService;
    
    public PhotoService(AlbumStructureService albumService)
    {
        _albumService = albumService;
        
        _photosBasePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "AIVanBan",
            "Photos"
        );
        
        _thumbnailCachePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "AIVanBan",
            "Cache",
            "Thumbnails"
        );
        
        Directory.CreateDirectory(_photosBasePath);
        Directory.CreateDirectory(_thumbnailCachePath);
    }
    
    #region Upload Photos
    
    /// <summary>
    /// Upload nhiều ảnh vào album
    /// </summary>
    public async Task<List<PhotoUploadResult>> UploadPhotos(string albumFolderPath, List<string> sourceFiles, IProgress<UploadProgress>? progress = null)
    {
        var results = new List<PhotoUploadResult>();
        int totalFiles = sourceFiles.Count;
        int processed = 0;
        
        foreach (var sourceFile in sourceFiles)
        {
            try
            {
                var result = await UploadSinglePhoto(albumFolderPath, sourceFile);
                results.Add(result);
                
                processed++;
                progress?.Report(new UploadProgress
                {
                    TotalFiles = totalFiles,
                    ProcessedFiles = processed,
                    CurrentFile = Path.GetFileName(sourceFile),
                    Success = result.Success
                });
            }
            catch (Exception ex)
            {
                results.Add(new PhotoUploadResult
                {
                    Success = false,
                    SourcePath = sourceFile,
                    ErrorMessage = ex.Message
                });
                
                processed++;
                progress?.Report(new UploadProgress
                {
                    TotalFiles = totalFiles,
                    ProcessedFiles = processed,
                    CurrentFile = Path.GetFileName(sourceFile),
                    Success = false,
                    ErrorMessage = ex.Message
                });
            }
        }
        
        return results;
    }
    
    private async Task<PhotoUploadResult> UploadSinglePhoto(string albumFolderPath, string sourceFile)
    {
        if (!File.Exists(sourceFile))
        {
            throw new FileNotFoundException("Source file not found", sourceFile);
        }
        
        // Kiểm tra định dạng file
        var extension = Path.GetExtension(sourceFile).ToLower();
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".heic", ".raw", ".cr2", ".nef" };
        
        if (!allowedExtensions.Contains(extension))
        {
            throw new NotSupportedException($"File format {extension} is not supported");
        }
        
        // Tạo tên file unique để tránh trùng
        var fileName = Path.GetFileName(sourceFile);
        var destinationPath = Path.Combine(albumFolderPath, fileName);
        
        // Nếu file đã tồn tại, thêm số vào tên
        int counter = 1;
        while (File.Exists(destinationPath))
        {
            var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            destinationPath = Path.Combine(albumFolderPath, $"{nameWithoutExt}_{counter}{extension}");
            counter++;
        }
        
        // Copy file
        await Task.Run(() => File.Copy(sourceFile, destinationPath, false));
        
        // Tạo thumbnail
        string? thumbnailPath = null;
        try
        {
            thumbnailPath = await CreateThumbnail(destinationPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not create thumbnail for {fileName}: {ex.Message}");
        }
        
        // Đọc metadata
        var metadata = ExtractMetadata(destinationPath);
        
        return new PhotoUploadResult
        {
            Success = true,
            SourcePath = sourceFile,
            DestinationPath = destinationPath,
            ThumbnailPath = thumbnailPath,
            Metadata = metadata,
            FileSize = new FileInfo(destinationPath).Length
        };
    }
    
    #endregion
    
    #region Thumbnail Management
    
    /// <summary>
    /// Tạo thumbnail 150x150px cho ảnh
    /// </summary>
    public async Task<string> CreateThumbnail(string imagePath, int size = 150)
    {
        return await Task.Run(() =>
        {
            var fileHash = ComputeFileHash(imagePath);
            var thumbnailFileName = $"{fileHash}_{size}.jpg";
            var thumbnailPath = Path.Combine(_thumbnailCachePath, thumbnailFileName);
            
            // Nếu thumbnail đã tồn tại, return luôn
            if (File.Exists(thumbnailPath))
            {
                return thumbnailPath;
            }
            
            // Tạo thumbnail mới
            using var original = Image.FromFile(imagePath);
            
            // Tính toán kích thước thumbnail giữ tỷ lệ
            int thumbWidth, thumbHeight;
            if (original.Width > original.Height)
            {
                thumbWidth = size;
                thumbHeight = (int)((float)original.Height / original.Width * size);
            }
            else
            {
                thumbHeight = size;
                thumbWidth = (int)((float)original.Width / original.Height * size);
            }
            
            using var thumbnail = new Bitmap(thumbWidth, thumbHeight);
            using var graphics = Graphics.FromImage(thumbnail);
            
            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            graphics.DrawImage(original, 0, 0, thumbWidth, thumbHeight);
            
            // Lưu thumbnail với quality 85%
            var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 85L);
            var jpegCodec = ImageCodecInfo.GetImageEncoders().First(c => c.FormatID == ImageFormat.Jpeg.Guid);
            
            thumbnail.Save(thumbnailPath, jpegCodec, encoderParams);
            
            return thumbnailPath;
        });
    }
    
    /// <summary>
    /// Lấy đường dẫn thumbnail (tạo nếu chưa có)
    /// </summary>
    public async Task<string> GetThumbnailPath(string imagePath)
    {
        var fileHash = ComputeFileHash(imagePath);
        var thumbnailPath = Path.Combine(_thumbnailCachePath, $"{fileHash}_150.jpg");
        
        if (!File.Exists(thumbnailPath))
        {
            return await CreateThumbnail(imagePath);
        }
        
        return thumbnailPath;
    }
    
    private string ComputeFileHash(string filePath)
    {
        using var md5 = MD5.Create();
        using var stream = File.OpenRead(filePath);
        var hashBytes = md5.ComputeHash(stream);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }
    
    #endregion
    
    #region Metadata Extraction
    
    /// <summary>
    /// Đọc metadata từ ảnh (EXIF)
    /// </summary>
    public PhotoMetadata ExtractMetadata(string imagePath)
    {
        var metadata = new PhotoMetadata
        {
            FileName = Path.GetFileName(imagePath),
            FilePath = imagePath,
            FileSize = new FileInfo(imagePath).Length,
            FileCreatedDate = File.GetCreationTime(imagePath),
            FileModifiedDate = File.GetLastWriteTime(imagePath)
        };
        
        try
        {
            using var image = Image.FromFile(imagePath);
            
            metadata.Width = image.Width;
            metadata.Height = image.Height;
            metadata.Resolution = $"{image.Width} x {image.Height}";
            
            // Đọc EXIF data
            // DateTaken
            if (image.PropertyIdList.Contains(36867)) // DateTimeOriginal
            {
                var propItem = image.GetPropertyItem(36867);
                var dateStr = System.Text.Encoding.ASCII.GetString(propItem.Value).Trim('\0');
                if (DateTime.TryParseExact(dateStr, "yyyy:MM:dd HH:mm:ss", null, 
                    System.Globalization.DateTimeStyles.None, out var dateTaken))
                {
                    metadata.DateTaken = dateTaken;
                }
            }
            
            // Camera Make
            if (image.PropertyIdList.Contains(271))
            {
                var propItem = image.GetPropertyItem(271);
                metadata.CameraMake = System.Text.Encoding.ASCII.GetString(propItem.Value).Trim('\0');
            }
            
            // Camera Model
            if (image.PropertyIdList.Contains(272))
            {
                var propItem = image.GetPropertyItem(272);
                metadata.CameraModel = System.Text.Encoding.ASCII.GetString(propItem.Value).Trim('\0');
            }
            
            // GPS Latitude
            if (image.PropertyIdList.Contains(2))
            {
                metadata.HasGPS = true;
                // TODO: Parse GPS coordinates properly
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not extract metadata from {imagePath}: {ex.Message}");
        }
        
        return metadata;
    }
    
    #endregion
    
    #region Photo Operations
    
    /// <summary>
    /// Lấy danh sách ảnh trong folder
    /// </summary>
    public List<string> GetPhotosInFolder(string folderPath, bool recursive = false)
    {
        if (!Directory.Exists(folderPath))
        {
            return new List<string>();
        }
        
        var extensions = new[] { "*.jpg", "*.jpeg", "*.png", "*.gif", "*.bmp" };
        var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        
        var photos = new List<string>();
        foreach (var ext in extensions)
        {
            photos.AddRange(Directory.GetFiles(folderPath, ext, searchOption));
        }
        
        return photos.OrderBy(p => p).ToList();
    }
    
    /// <summary>
    /// Xóa ảnh
    /// </summary>
    public bool DeletePhoto(string imagePath)
    {
        try
        {
            if (File.Exists(imagePath))
            {
                File.Delete(imagePath);
                
                // Xóa thumbnail nếu có
                var fileHash = ComputeFileHash(imagePath);
                var thumbnailPath = Path.Combine(_thumbnailCachePath, $"{fileHash}_150.jpg");
                if (File.Exists(thumbnailPath))
                {
                    File.Delete(thumbnailPath);
                }
                
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting photo {imagePath}: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Di chuyển ảnh sang album khác
    /// </summary>
    public bool MovePhoto(string sourcePath, string destinationFolderPath)
    {
        try
        {
            if (!File.Exists(sourcePath))
            {
                throw new FileNotFoundException("Source file not found", sourcePath);
            }
            
            Directory.CreateDirectory(destinationFolderPath);
            
            var fileName = Path.GetFileName(sourcePath);
            var destinationPath = Path.Combine(destinationFolderPath, fileName);
            
            // Nếu file đã tồn tại, thêm số vào tên
            int counter = 1;
            while (File.Exists(destinationPath))
            {
                var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                var extension = Path.GetExtension(fileName);
                destinationPath = Path.Combine(destinationFolderPath, $"{nameWithoutExt}_{counter}{extension}");
                counter++;
            }
            
            File.Move(sourcePath, destinationPath);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error moving photo: {ex.Message}");
            return false;
        }
    }
    
    #endregion
    
    public void Dispose()
    {
        // Cleanup if needed
    }
}

#region Supporting Classes

public class PhotoUploadResult
{
    public bool Success { get; set; }
    public string SourcePath { get; set; } = "";
    public string DestinationPath { get; set; } = "";
    public string? ThumbnailPath { get; set; }
    public string? ErrorMessage { get; set; }
    public long FileSize { get; set; }
    public PhotoMetadata? Metadata { get; set; }
}

public class UploadProgress
{
    public int TotalFiles { get; set; }
    public int ProcessedFiles { get; set; }
    public string CurrentFile { get; set; } = "";
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public double PercentComplete => TotalFiles > 0 ? (double)ProcessedFiles / TotalFiles * 100 : 0;
}

public class PhotoMetadata
{
    public string FileName { get; set; } = "";
    public string FilePath { get; set; } = "";
    public long FileSize { get; set; }
    public DateTime FileCreatedDate { get; set; }
    public DateTime FileModifiedDate { get; set; }
    
    public int Width { get; set; }
    public int Height { get; set; }
    public string Resolution { get; set; } = "";
    
    public DateTime? DateTaken { get; set; }
    public string? CameraMake { get; set; }
    public string? CameraModel { get; set; }
    
    public bool HasGPS { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    
    public string FileSizeFormatted => FormatFileSize(FileSize);
    
    private string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}

#endregion
