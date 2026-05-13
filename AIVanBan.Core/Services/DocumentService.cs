using LiteDB;
using AIVanBan.Core.Models;
using AIVanBan.Core.Data;

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
        _dataPath = databasePath ?? DatabaseFactory.DataPath;
        
        Directory.CreateDirectory(_dataPath);
        
        // Dùng shared database instance — tránh file lock conflict
        _db = DatabaseFactory.GetDatabase(databasePath);
        
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
    
    /// <summary>
    /// Xóa toàn bộ văn bản (bao gồm cả thùng rác) — dùng cho reset demo data
    /// </summary>
    public int DeleteAllDocuments()
    {
        var collection = _db.GetCollection<Document>("documents");
        var count = collection.Count();
        _db.DropCollection("documents");
        
        // Tái tạo indexes
        var newCollection = _db.GetCollection<Document>("documents");
        newCollection.EnsureIndex(x => x.Title);
        newCollection.EnsureIndex(x => x.Number);
        newCollection.EnsureIndex(x => x.Direction);
        newCollection.EnsureIndex(x => x.Type);
        
        return count;
    }
    
    public Document AddDocument(Document document)
    {
        var collection = _db.GetCollection<Document>("documents");
        collection.Insert(document);
        SyncTracker.MarkChanged(document.Id, SyncTracker.EntityTypes.Document);
        return document;
    }
    
    public bool UpdateDocument(Document document)
    {
        document.ModifiedBy = Environment.UserName;
        document.ModifiedDate = DateTime.Now;
        
        var collection = _db.GetCollection<Document>("documents");
        var result = collection.Update(document);
        if (result) SyncTracker.MarkChanged(document.Id, SyncTracker.EntityTypes.Document);
        return result;
    }
    
    public bool DeleteDocument(string id)
    {
        // Cascade delete: xóa tất cả file đính kèm trước — tránh orphan files
        try
        {
            var attachments = GetAttachmentsByDocument(id);
            foreach (var att in attachments)
            {
                DeleteAttachment(att.Id);
            }
            
            // Xóa thư mục đính kèm của document nếu còn tồn tại
            var attachmentDir = Path.Combine(_dataPath, "Attachments", id);
            if (Directory.Exists(attachmentDir))
            {
                try { Directory.Delete(attachmentDir, recursive: true); } catch { }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Error cleaning up attachments for doc {id}: {ex.Message}");
        }
        
        var collection = _db.GetCollection<Document>("documents");
        var result = collection.Delete(id);
        if (result) SyncTracker.MarkDeleted(id, SyncTracker.EntityTypes.Document);
        return result;
    }
    
    public Document? GetDocument(string id)
    {
        var collection = _db.GetCollection<Document>("documents");
        return collection.FindById(id);
    }
    
    public List<Document> GetAllDocuments()
    {
        var collection = _db.GetCollection<Document>("documents");
        // Lọc ở tầng C# vì document cũ chưa có field IsDeleted trong BSON
        return collection.FindAll().Where(d => !d.IsDeleted).ToList();
    }
    
    #endregion
    
    #region Search & Filter
    
    public List<Document> SearchDocuments(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword)) return GetAllDocuments();
        
        var collection = _db.GetCollection<Document>("documents");
        return collection.Find(d => 
            (d.Title ?? "").Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
            (d.Number ?? "").Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
            (d.Subject ?? "").Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
            (d.Content ?? "").Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
            (d.Issuer ?? "").Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
            (d.SignedBy ?? "").Contains(keyword, StringComparison.OrdinalIgnoreCase)
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
    
    #region Deadline Alerts — Theo dõi hạn xử lý VB đến (Điều 24, NĐ 30/2020)
    
    /// <summary>
    /// Lấy danh sách VB đến đã quá hạn xử lý.
    /// Điều kiện: Direction==Den, DueDate < Today, chưa Archived/Published.
    /// </summary>
    public List<Document> GetOverdueDocuments()
    {
        var collection = _db.GetCollection<Document>("documents");
        var today = DateTime.Today;
        return collection.Find(d =>
            d.Direction == Direction.Den &&
            d.DueDate.HasValue &&
            d.DueDate.Value.Date < today &&
            d.WorkflowStatus != DocumentStatus.Archived &&
            d.WorkflowStatus != DocumentStatus.Published
        ).Where(d => !d.IsDeleted).ToList();
    }
    
    /// <summary>
    /// Lấy danh sách VB đến sắp hết hạn xử lý (trong N ngày tới).
    /// </summary>
    public List<Document> GetDocumentsDueSoon(int withinDays = 3)
    {
        var collection = _db.GetCollection<Document>("documents");
        var today = DateTime.Today;
        var deadline = today.AddDays(withinDays);
        return collection.Find(d =>
            d.Direction == Direction.Den &&
            d.DueDate.HasValue &&
            d.DueDate.Value.Date >= today &&
            d.DueDate.Value.Date <= deadline &&
            d.WorkflowStatus != DocumentStatus.Archived &&
            d.WorkflowStatus != DocumentStatus.Published
        ).Where(d => !d.IsDeleted).ToList();
    }
    
    #endregion
    
    #region Soft Delete — Thùng rác
    
    /// <summary>
    /// Xóa mềm văn bản (chuyển vào thùng rác)
    /// </summary>
    public bool SoftDeleteDocument(string id)
    {
        var collection = _db.GetCollection<Document>("documents");
        var doc = collection.FindById(id);
        if (doc == null) return false;
        
        doc.IsDeleted = true;
        doc.DeletedDate = DateTime.Now;
        doc.DeletedBy = Environment.UserName;
        var result = collection.Update(doc);
        if (result) SyncTracker.MarkChanged(doc.Id, SyncTracker.EntityTypes.Document);
        return result;
    }
    
    /// <summary>
    /// Khôi phục văn bản từ thùng rác
    /// </summary>
    public bool RestoreDocument(string id)
    {
        var collection = _db.GetCollection<Document>("documents");
        var doc = collection.FindById(id);
        if (doc == null) return false;
        
        doc.IsDeleted = false;
        doc.DeletedDate = null;
        doc.DeletedBy = null;
        var result = collection.Update(doc);
        if (result) SyncTracker.MarkChanged(doc.Id, SyncTracker.EntityTypes.Document);
        return result;
    }
    
    /// <summary>
    /// Lấy tất cả VB trong thùng rác
    /// </summary>
    public List<Document> GetDeletedDocuments()
    {
        var collection = _db.GetCollection<Document>("documents");
        return collection.FindAll().Where(d => d.IsDeleted).ToList();
    }
    
    /// <summary>
    /// Xóa vĩnh viễn văn bản (cascade delete attachments + files)
    /// </summary>
    public bool PermanentDeleteDocument(string id)
    {
        // Reuse existing hard delete logic
        return DeleteDocument(id);
    }
    
    /// <summary>
    /// Dọn sạch thùng rác (xóa vĩnh viễn tất cả VB đã xóa mềm)
    /// </summary>
    public int EmptyTrash()
    {
        var deleted = GetDeletedDocuments();
        int count = 0;
        foreach (var doc in deleted)
        {
            if (DeleteDocument(doc.Id)) count++;
        }
        return count;
    }
    
    /// <summary>
    /// Lấy số lượng VB trong thùng rác
    /// </summary>
    public int GetTrashCount()
    {
        var collection = _db.GetCollection<Document>("documents");
        return collection.FindAll().Count(d => d.IsDeleted);
    }
    
    #endregion
    
    #region Sổ theo dõi cá nhân — Personal Tracking
    
    /// <summary>Cập nhật trạng thái xử lý cá nhân</summary>
    public bool UpdatePersonalStatus(string docId, PersonalStatus status)
    {
        var doc = GetDocument(docId);
        if (doc == null) return false;
        doc.MyStatus = status;
        doc.ModifiedDate = DateTime.Now;
        return UpdateDocument(doc);
    }

    /// <summary>Toggle đánh dấu sao</summary>
    public bool ToggleStar(string docId)
    {
        var doc = GetDocument(docId);
        if (doc == null) return false;
        doc.IsStarred = !doc.IsStarred;
        doc.ModifiedDate = DateTime.Now;
        return UpdateDocument(doc);
    }

    /// <summary>Thêm ghi chú bút phê vào VB</summary>
    public bool AddNote(string docId, string content, NoteType type = NoteType.ButPhe)
    {
        var doc = GetDocument(docId);
        if (doc == null) return false;
        doc.Notes ??= new List<PersonalNoteEntry>();
        doc.Notes.Add(new PersonalNoteEntry
        {
            Content = content,
            Type = type,
            CreatedDate = DateTime.Now
        });
        doc.ModifiedDate = DateTime.Now;
        return UpdateDocument(doc);
    }

    /// <summary>Xóa ghi chú bút phê</summary>
    public bool DeleteNote(string docId, string noteId)
    {
        var doc = GetDocument(docId);
        if (doc == null) return false;
        doc.Notes ??= new List<PersonalNoteEntry>();
        var removed = doc.Notes.RemoveAll(n => n.Id == noteId);
        if (removed == 0) return false;
        doc.ModifiedDate = DateTime.Now;
        return UpdateDocument(doc);
    }

    /// <summary>Lấy VB đánh dấu sao</summary>
    public List<Document> GetStarredDocuments()
    {
        var collection = _db.GetCollection<Document>("documents");
        return collection.FindAll()
            .Where(d => !d.IsDeleted && d.IsStarred)
            .OrderByDescending(d => d.ModifiedDate ?? d.CreatedDate)
            .ToList();
    }

    /// <summary>Lấy VB chưa xử lý</summary>
    public List<Document> GetUnprocessedDocuments()
    {
        var collection = _db.GetCollection<Document>("documents");
        return collection.FindAll()
            .Where(d => !d.IsDeleted && d.MyStatus == PersonalStatus.ChuaXuLy)
            .OrderByDescending(d => d.IssueDate)
            .ToList();
    }

    /// <summary>Lấy VB quá hạn cá nhân (PersonalDeadline hoặc DueDate đã qua)</summary>
    public List<Document> GetPersonalOverdueDocuments()
    {
        var now = DateTime.Now;
        var collection = _db.GetCollection<Document>("documents");
        return collection.FindAll()
            .Where(d => !d.IsDeleted 
                && d.MyStatus != PersonalStatus.DaXuLy 
                && d.MyStatus != PersonalStatus.ChuyenTiep
                && ((d.PersonalDeadline.HasValue && d.PersonalDeadline.Value < now)
                    || (d.DueDate.HasValue && d.DueDate.Value < now)))
            .OrderBy(d => d.PersonalDeadline ?? d.DueDate ?? d.IssueDate)
            .ToList();
    }

    #endregion
    
    #region Auto-increment số VB — Theo Điều 15, NĐ 30/2020/NĐ-CP
    
    /// <summary>
    /// Lấy số VB tiếp theo cho loại VB và hướng trong năm hiện tại.
    /// Số bắt đầu từ 01, liên tiếp, reset mỗi đầu năm (01/01).
    /// Văn bản mật có hệ thống số riêng (Điều 15 khoản 2).
    /// </summary>
    public int GetNextDocumentNumber(Direction direction, int? year = null, bool isSecret = false)
    {
        var targetYear = year ?? DateTime.Now.Year;
        var collection = _db.GetCollection<Document>("documents");
        
        // Lọc theo hướng và năm
        var docsInYear = collection.Find(d => 
            d.Direction == direction && 
            d.IssueDate.Year == targetYear
        ).ToList();
        
        // Văn bản mật có hệ thống số riêng
        if (isSecret)
        {
            docsInYear = docsInYear.Where(d => d.SecurityLevel != SecurityLevel.Thuong).ToList();
        }
        else
        {
            docsInYear = docsInYear.Where(d => d.SecurityLevel == SecurityLevel.Thuong).ToList();
        }
        
        if (docsInYear.Count == 0)
            return 1;
        
        // Tìm số lớn nhất đã cấp
        var maxNumber = 0;
        foreach (var doc in docsInYear)
        {
            if (string.IsNullOrEmpty(doc.Number)) continue;
            // Trích xuất phần số từ ký hiệu "123/CV-UBND" → 123
            var numStr = doc.Number.Split('/').FirstOrDefault()?.Trim();
            if (int.TryParse(numStr, out var num) && num > maxNumber)
                maxNumber = num;
        }
        
        return maxNumber + 1;
    }
    
    /// <summary>
    /// Tạo ký hiệu VB hoàn chỉnh: "Số/Loại-CQ" (VD: "15/QĐ-UBND")
    /// Theo Phụ lục III, NĐ 30/2020/NĐ-CP
    /// </summary>
    public string GenerateDocumentSymbol(DocumentType type, string orgAbbreviation, Direction direction, int? year = null, bool isSecret = false)
    {
        var nextNum = GetNextDocumentNumber(direction, year, isSecret);
        var typeAbbr = type.GetAbbreviation();
        
        if (string.IsNullOrEmpty(typeAbbr))
            return $"{nextNum}";
        
        if (string.IsNullOrEmpty(orgAbbreviation))
            return $"{nextNum}/{typeAbbr}";
        
        return $"{nextNum}/{typeAbbr}-{orgAbbreviation}";
    }
    
    /// <summary>
    /// Lấy số đến tiếp theo (VB đến, liên tiếp trong năm) — Điều 22 NĐ 30/2020
    /// </summary>
    public int GetNextArrivalNumber(int? year = null)
    {
        var targetYear = year ?? DateTime.Now.Year;
        var collection = _db.GetCollection<Document>("documents");
        
        var maxArrival = collection.Find(d => 
            d.Direction == Direction.Den && 
            d.ArrivalDate.HasValue &&
            d.ArrivalDate.Value.Year == targetYear
        ).Select(d => d.ArrivalNumber).DefaultIfEmpty(0).Max();
        
        return maxArrival + 1;
    }
    
    #endregion
    
    #region Sao văn bản — Theo Điều 25-27, NĐ 30/2020/NĐ-CP
    
    /// <summary>
    /// Lấy số bản sao tiếp theo trong năm.
    /// Theo NĐ 30/2020 Phụ lục I: Số bản sao đánh chung cho tất cả loại bản sao (SY/SL/TrS),
    /// bắt đầu từ 01 ngày 01/01 và kết thúc ngày 31/12 hàng năm.
    /// </summary>
    public int GetNextCopyNumber(int? year = null)
    {
        var collection = _db.GetCollection<Document>("documents");
        var targetYear = year ?? DateTime.Now.Year;
        
        var maxCopyNum = collection.Find(d => 
            d.CopyType != CopyType.None &&
            d.CopyDate.HasValue &&
            d.CopyDate.Value.Year == targetYear
        ).Select(d => d.CopyNumber).DefaultIfEmpty(0).Max();
        
        return maxCopyNum + 1;
    }
    
    /// <summary>
    /// Tạo ký hiệu bản sao: Số/Loại-CQ (VD: 05/SY-UBND, 12/TrS-BNV)
    /// Theo Phụ lục I, Phần II, NĐ 30/2020/NĐ-CP
    /// </summary>
    public string GenerateCopySymbol(CopyType copyType, string orgAbbreviation)
    {
        var nextNum = GetNextCopyNumber();
        var abbr = copyType.GetAbbreviation();
        return $"{nextNum:D2}/{abbr}-{orgAbbreviation}";
    }
    
    /// <summary>
    /// Sao văn bản — Theo Điều 25, NĐ 30/2020/NĐ-CP.
    /// Tạo bản sao (sao y / sao lục / trích sao) từ văn bản gốc.
    /// - Sao y: sao đầy đủ từ bản gốc/bản chính (Điều 25 khoản 1)
    /// - Sao lục: sao từ bản sao y (Điều 25 khoản 2)
    /// - Trích sao: sao phần nội dung cần trích (Điều 25 khoản 3)
    /// Bản sao có giá trị pháp lý như bản chính (Điều 26).
    /// </summary>
    public Document CopyDocument(string originalDocId, CopyType copyType, string orgAbbreviation,
        string copiedBy, string signingTitle, string[] recipients, string? extractedContent = null)
    {
        var original = GetDocument(originalDocId);
        if (original == null) throw new ArgumentException($"Không tìm thấy văn bản gốc: {originalDocId}");
        
        // Sao lục phải từ bản sao y (Điều 25 khoản 2)
        if (copyType == CopyType.SaoLuc && original.CopyType != CopyType.SaoY && original.CopyType != CopyType.None)
            throw new ArgumentException("Sao lục chỉ được thực hiện từ bản sao y hoặc bản gốc (Điều 25 khoản 2).");
        
        var copyNum = GetNextCopyNumber();
        var abbr = copyType.GetAbbreviation();
        var symbol = $"{copyNum:D2}/{abbr}-{orgAbbreviation}";
        
        var copy = new Document
        {
            // Giữ nguyên thông tin VB gốc
            Title = original.Title,
            Number = original.Number,
            IssueDate = original.IssueDate,
            Issuer = original.Issuer,
            Subject = original.Subject,
            Type = original.Type,
            Category = original.Category,
            Direction = original.Direction,
            UrgencyLevel = original.UrgencyLevel,
            SecurityLevel = original.SecurityLevel,
            Content = copyType == CopyType.TrichSao && !string.IsNullOrEmpty(extractedContent) 
                ? extractedContent 
                : original.Content ?? "",
            Tags = (original.Tags ?? Array.Empty<string>()).ToArray(),
            FolderId = original.FolderId ?? "",
            Status = original.Status ?? "Còn hiệu lực",
            BasedOn = (original.BasedOn ?? Array.Empty<string>()).ToArray(),
            
            // Thông tin bản sao
            CopyType = copyType,
            OriginalDocumentId = original.CopyType == CopyType.None ? originalDocId : original.OriginalDocumentId,
            CopyNumber = copyNum,
            CopySymbol = symbol,
            CopyDate = DateTime.Now,
            CopiedBy = copiedBy,
            CopySigningTitle = signingTitle,
            CopyNotes = copyType == CopyType.TrichSao ? extractedContent ?? "" : "",
            Recipients = recipients,
            
            // Metadata bản sao
            CreatedBy = Environment.UserName,
            CreatedDate = DateTime.Now,
        };
        
        var collection = _db.GetCollection<Document>("documents");
        collection.Insert(copy);
        
        return copy;
    }
    
    /// <summary>
    /// Lấy tất cả bản sao của một văn bản gốc
    /// </summary>
    public List<Document> GetCopiesOfDocument(string originalDocId)
    {
        var collection = _db.GetCollection<Document>("documents");
        return collection.Find(d => d.OriginalDocumentId == originalDocId && d.CopyType != CopyType.None).ToList();
    }
    
    #endregion
    
    #region Templates
    
    public DocumentTemplate AddTemplate(DocumentTemplate template)
    {
        var collection = _db.GetCollection<DocumentTemplate>("templates");
        collection.Insert(template);
        SyncTracker.MarkChanged(template.Id, SyncTracker.EntityTypes.Template);
        return template;
    }
    
    /// <summary>
    /// Bulk insert templates — much faster than individual inserts for first-run seeding.
    /// </summary>
    public int BulkInsertTemplates(IEnumerable<DocumentTemplate> templates)
    {
        var collection = _db.GetCollection<DocumentTemplate>("templates");
        return collection.InsertBulk(templates);
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
        SyncTracker.MarkChanged(template.Id, SyncTracker.EntityTypes.Template);
    }
    
    public void DeleteTemplate(string id)
    {
        var collection = _db.GetCollection<DocumentTemplate>("templates");
        collection.Delete(id);
        SyncTracker.MarkDeleted(id, SyncTracker.EntityTypes.Template);
    }
    
    /// <summary>
    /// Xóa toàn bộ collection templates (dùng khi data bị corrupted/enum mismatch)
    /// </summary>
    public void DropTemplatesCollection()
    {
        _db.DropCollection("templates");
        Console.WriteLine("🗑️ Dropped templates collection.");
    }
    
    #endregion
    
    #region Photos & Albums
    
    public Photo AddPhoto(Photo photo)
    {
        var collection = _db.GetCollection<Photo>("photos");
        collection.Insert(photo);
        SyncTracker.MarkChanged(photo.Id, SyncTracker.EntityTypes.Photo);
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
        SyncTracker.MarkChanged(album.Id, SyncTracker.EntityTypes.Album);
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
        SyncTracker.MarkChanged(photo.Id, SyncTracker.EntityTypes.Photo);
    }
    
    public void DeletePhoto(string id)
    {
        var collection = _db.GetCollection<Photo>("photos");
        collection.Delete(id);
        SyncTracker.MarkDeleted(id, SyncTracker.EntityTypes.Photo);
    }
    
    public void UpdateAlbum(Album album)
    {
        var collection = _db.GetCollection<Album>("albums");
        collection.Update(album);
        SyncTracker.MarkChanged(album.Id, SyncTracker.EntityTypes.Album);
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
        SyncTracker.MarkChanged(folder.Id, SyncTracker.EntityTypes.Folder);
    }

    public void UpdateFolder(Folder folder)
    {
        var folders = _db.GetCollection<Folder>("folders");
        folders.Update(folder);
        SyncTracker.MarkChanged(folder.Id, SyncTracker.EntityTypes.Folder);
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
        SyncTracker.MarkDeleted(id, SyncTracker.EntityTypes.Folder);
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
        // Không dispose _db — DatabaseFactory quản lý vòng đời shared instance
    }
}
