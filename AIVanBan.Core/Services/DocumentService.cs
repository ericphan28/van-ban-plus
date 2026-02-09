using LiteDB;
using AIVanBan.Core.Models;

namespace AIVanBan.Core.Services;

/// <summary>
/// Service quản lý tài liệu với LiteDB
/// </summary>
public class DocumentService : IDisposable
{
    private readonly LiteDatabase _db;
    private readonly string _dataPath;
    
    public DocumentService(string? databasePath = null)
    {
        // Mặc định lưu trong My Documents
        _dataPath = databasePath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "AIVanBan",
            "Data"
        );
        
        Directory.CreateDirectory(_dataPath);
        
        var dbPath = Path.Combine(_dataPath, "documents.db");
        _db = new LiteDatabase($"Filename={dbPath};Connection=Shared");
        
        // Tạo indexes
        var documents = _db.GetCollection<Document>("documents");
        documents.EnsureIndex(x => x.Title);
        documents.EnsureIndex(x => x.Number);
        documents.EnsureIndex(x => x.Type);
        documents.EnsureIndex(x => x.IssueDate);
        documents.EnsureIndex(x => x.Tags);
        documents.EnsureIndex("$Content"); // Full-text search
    }
    
    #region CRUD Documents
    
    public Document AddDocument(Document document)
    {
        var collection = _db.GetCollection<Document>("documents");
        collection.Insert(document);
        return document;
    }
    
    public bool UpdateDocument(Document document)
    {
        document.ModifiedBy = Environment.UserName;
        document.ModifiedDate = DateTime.Now;
        
        var collection = _db.GetCollection<Document>("documents");
        return collection.Update(document);
    }
    
    public bool DeleteDocument(string id)
    {
        var collection = _db.GetCollection<Document>("documents");
        return collection.Delete(id);
    }
    
    public Document? GetDocument(string id)
    {
        var collection = _db.GetCollection<Document>("documents");
        return collection.FindById(id);
    }
    
    public List<Document> GetAllDocuments()
    {
        var collection = _db.GetCollection<Document>("documents");
        return collection.FindAll().ToList();
    }
    
    #endregion
    
    #region Search & Filter
    
    public List<Document> SearchDocuments(string keyword)
    {
        var collection = _db.GetCollection<Document>("documents");
        return collection.Find(d => 
            d.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
            d.Number.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
            d.Subject.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
            d.Content.Contains(keyword, StringComparison.OrdinalIgnoreCase)
        ).ToList();
    }
    
    public List<Document> GetDocumentsByType(DocumentType type)
    {
        var collection = _db.GetCollection<Document>("documents");
        return collection.Find(d => d.Type == type).ToList();
    }
    
    public List<Document> GetDocumentsByDirection(Direction direction)
    {
        var collection = _db.GetCollection<Document>("documents");
        return collection.Find(d => d.Direction == direction).ToList();
    }
    
    public List<Document> GetDocumentsByYear(int year)
    {
        var collection = _db.GetCollection<Document>("documents");
        return collection.Find(d => d.IssueDate.Year == year).ToList();
    }
    
    public List<Document> GetDocumentsByDateRange(DateTime from, DateTime to)
    {
        var collection = _db.GetCollection<Document>("documents");
        return collection.Find(d => d.IssueDate >= from && d.IssueDate <= to).ToList();
    }
    
    public List<Document> GetDocumentsByFolder(string folderId)
    {
        if (string.IsNullOrEmpty(folderId))
            return GetAllDocuments();
            
        var collection = _db.GetCollection<Document>("documents");
        return collection.Find(d => d.FolderId == folderId).ToList();
    }
    
    #endregion
    
    #region Statistics
    
    public int GetTotalDocuments()
    {
        var collection = _db.GetCollection<Document>("documents");
        return collection.Count();
    }
    
    public Dictionary<DocumentType, int> GetDocumentCountByType()
    {
        var documents = GetAllDocuments();
        return documents.GroupBy(d => d.Type)
                       .ToDictionary(g => g.Key, g => g.Count());
    }
    
    public Dictionary<int, int> GetDocumentCountByYear()
    {
        var documents = GetAllDocuments();
        return documents.GroupBy(d => d.IssueDate.Year)
                       .OrderByDescending(g => g.Key)
                       .ToDictionary(g => g.Key, g => g.Count());
    }
    
    #endregion
    
    #region Templates
    
    public DocumentTemplate AddTemplate(DocumentTemplate template)
    {
        var collection = _db.GetCollection<DocumentTemplate>("templates");
        collection.Insert(template);
        return template;
    }
    
    public List<DocumentTemplate> GetAllTemplates()
    {
        var collection = _db.GetCollection<DocumentTemplate>("templates");
        return collection.FindAll().ToList();
    }
    
    public DocumentTemplate? GetTemplateById(string id)
    {
        var collection = _db.GetCollection<DocumentTemplate>("templates");
        return collection.FindById(id);
    }
    
    public List<DocumentTemplate> GetTemplatesByType(DocumentType type)
    {
        var collection = _db.GetCollection<DocumentTemplate>("templates");
        return collection.Find(t => t.Type == type).ToList();
    }
    
    public void UpdateTemplate(DocumentTemplate template)
    {
        var collection = _db.GetCollection<DocumentTemplate>("templates");
        collection.Update(template);
    }
    
    public void DeleteTemplate(string id)
    {
        var collection = _db.GetCollection<DocumentTemplate>("templates");
        collection.Delete(id);
    }
    
    #endregion
    
    #region Photos & Albums
    
    public Photo AddPhoto(Photo photo)
    {
        var collection = _db.GetCollection<Photo>("photos");
        collection.Insert(photo);
        return photo;
    }
    
    public List<Photo> GetAllPhotos()
    {
        var collection = _db.GetCollection<Photo>("photos");
        return collection.FindAll().ToList();
    }
    
    public List<Photo> GetPhotosByAlbum(string albumId)
    {
        var collection = _db.GetCollection<Photo>("photos");
        return collection.Find(p => p.AlbumId == albumId).ToList();
    }
    
    public Album AddAlbum(Album album)
    {
        var collection = _db.GetCollection<Album>("albums");
        collection.Insert(album);
        return album;
    }
    
    public List<Album> GetAllAlbums()
    {
        var collection = _db.GetCollection<Album>("albums");
        return collection.FindAll().ToList();
    }
    
    public void UpdatePhoto(Photo photo)
    {
        var collection = _db.GetCollection<Photo>("photos");
        collection.Update(photo);
    }
    
    public void DeletePhoto(string id)
    {
        var collection = _db.GetCollection<Photo>("photos");
        collection.Delete(id);
    }
    
    public void UpdateAlbum(Album album)
    {
        var collection = _db.GetCollection<Album>("albums");
        collection.Update(album);
    }
    
    #endregion
    
    #region Config
    
    public OrganizationConfig GetOrganizationConfig()
    {
        var collection = _db.GetCollection<OrganizationConfig>("config");
        return collection.FindById("default") ?? new OrganizationConfig();
    }
    
    public void SaveOrganizationConfig(OrganizationConfig config)
    {
        config.Id = "default";
        var collection = _db.GetCollection<OrganizationConfig>("config");
        collection.Upsert(config);
    }
    
    #endregion
    
    #region Folder Management
    
    public List<Folder> GetAllFolders()
    {
        var folders = _db.GetCollection<Folder>("folders");
        return folders.FindAll().ToList();
    }

    public List<Folder> GetRootFolders()
    {
        var folders = _db.GetCollection<Folder>("folders");
        return folders.Find(f => string.IsNullOrEmpty(f.ParentId)).ToList();
    }

    public List<Folder> GetChildFolders(string parentId)
    {
        var folders = _db.GetCollection<Folder>("folders");
        return folders.Find(f => f.ParentId == parentId).ToList();
    }

    public Folder? GetFolderById(string id)
    {
        var folders = _db.GetCollection<Folder>("folders");
        return folders.FindById(id);
    }

    public void CreateFolder(Folder folder)
    {
        // Build path
        if (!string.IsNullOrEmpty(folder.ParentId))
        {
            var parent = GetFolderById(folder.ParentId);
            folder.Path = string.IsNullOrEmpty(parent?.Path) 
                ? folder.Name 
                : $"{parent.Path}/{folder.Name}";
        }
        else
        {
            folder.Path = folder.Name;
        }

        var folders = _db.GetCollection<Folder>("folders");
        folders.Insert(folder);
    }

    public void UpdateFolder(Folder folder)
    {
        var folders = _db.GetCollection<Folder>("folders");
        folders.Update(folder);
    }

    public void DeleteFolder(string id)
    {
        // Delete all child folders recursively
        var children = GetChildFolders(id);
        foreach (var child in children)
        {
            DeleteFolder(child.Id);
        }

        var folders = _db.GetCollection<Folder>("folders");
        folders.Delete(id);
    }

    public void InitializeDefaultFolders()
    {
        var folders = _db.GetCollection<Folder>("folders");
        if (folders.Count() > 0) return; // Already initialized

        var roots = new[]
        {
            new Folder { Name = "Văn bản đến", Icon = "📥" },
            new Folder { Name = "Văn bản đi", Icon = "📤" },
            new Folder { Name = "Văn bản nội bộ", Icon = "📋" },
            new Folder { Name = "Lưu trữ", Icon = "📦" }
        };

        foreach (var root in roots)
        {
            CreateFolder(root);
        }
    }
    
    #endregion
    
    #region Attachment Management
    
    /// <summary>
    /// Thêm file đính kèm cho văn bản
    /// </summary>
    public Attachment AddAttachment(Attachment attachment)
    {
        var collection = _db.GetCollection<Attachment>("attachments");
        collection.Insert(attachment);
        return attachment;
    }
    
    /// <summary>
    /// Lấy tất cả file đính kèm của một văn bản
    /// </summary>
    public List<Attachment> GetAttachmentsByDocument(string documentId)
    {
        var collection = _db.GetCollection<Attachment>("attachments");
        return collection.Find(a => a.DocumentId == documentId).ToList();
    }
    
    /// <summary>
    /// Lấy một file đính kèm
    /// </summary>
    public Attachment? GetAttachment(string attachmentId)
    {
        var collection = _db.GetCollection<Attachment>("attachments");
        return collection.FindById(attachmentId);
    }
    
    /// <summary>
    /// Xóa file đính kèm
    /// </summary>
    public bool DeleteAttachment(string attachmentId)
    {
        var collection = _db.GetCollection<Attachment>("attachments");
        var attachment = collection.FindById(attachmentId);
        
        if (attachment != null)
        {
            // Xóa file vật lý
            if (File.Exists(attachment.FilePath))
            {
                try
                {
                    File.Delete(attachment.FilePath);
                }
                catch
                {
                    // Ignore file deletion errors
                }
            }
            
            // Xóa record trong database
            return collection.Delete(attachmentId);
        }
        
        return false;
    }
    
    /// <summary>
    /// Lưu file đính kèm vào thư mục Attachments
    /// </summary>
    public string SaveAttachmentFile(string sourceFilePath, string documentId)
    {
        var attachmentDir = Path.Combine(_dataPath, "Attachments", documentId);
        Directory.CreateDirectory(attachmentDir);
        
        var fileName = Path.GetFileName(sourceFilePath);
        var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
        var destPath = Path.Combine(attachmentDir, uniqueFileName);
        
        File.Copy(sourceFilePath, destPath, overwrite: true);
        
        return destPath;
    }
    
    #endregion
    
    public void Dispose()
    {
        _db?.Dispose();
    }
}
