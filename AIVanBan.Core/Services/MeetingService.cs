using LiteDB;
using AIVanBan.Core.Models;
using AIVanBan.Core.Data;

namespace AIVanBan.Core.Services;

/// <summary>
/// Service quản lý cuộc họp với LiteDB
/// Dùng chung database với DocumentService (documents.db)
/// </summary>
public class MeetingService : IDisposable
{
    private readonly LiteDatabase _db;
    
    public MeetingService(string? databasePath = null)
    {
        var dataPath = databasePath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "AIVanBan",
            "Data"
        );
        
        Directory.CreateDirectory(dataPath);
        
        var dbPath = Path.Combine(dataPath, "documents.db");
        LiteDbConfig.ConfigureGlobalMapper();
        _db = new LiteDatabase($"Filename={dbPath};Connection=Shared");
        
        // Tạo indexes cho collection meetings
        var meetings = _db.GetCollection<Meeting>("meetings");
        meetings.EnsureIndex(x => x.Title);
        meetings.EnsureIndex(x => x.StartTime);
        meetings.EnsureIndex(x => x.Type);
        meetings.EnsureIndex(x => x.Status);
        meetings.EnsureIndex(x => x.Tags);
        meetings.EnsureIndex(x => x.ChairPerson);
    }
    
    #region CRUD
    
    public Meeting AddMeeting(Meeting meeting)
    {
        var collection = _db.GetCollection<Meeting>("meetings");
        collection.Insert(meeting);
        return meeting;
    }
    
    public bool UpdateMeeting(Meeting meeting)
    {
        meeting.ModifiedBy = Environment.UserName;
        meeting.ModifiedDate = DateTime.Now;
        
        // Auto-update overdue tasks
        AutoUpdateOverdueTasks(meeting);
        
        var collection = _db.GetCollection<Meeting>("meetings");
        return collection.Update(meeting);
    }
    
    public bool DeleteMeeting(string id)
    {
        var collection = _db.GetCollection<Meeting>("meetings");
        return collection.Delete(id);
    }
    
    public Meeting? GetMeetingById(string id)
    {
        var collection = _db.GetCollection<Meeting>("meetings");
        return collection.FindById(id);
    }
    
    #endregion
    
    #region Search & Filter
    
    /// <summary>
    /// Lấy tất cả cuộc họp, sắp xếp theo ngày gần nhất
    /// </summary>
    public List<Meeting> GetAllMeetings()
    {
        var collection = _db.GetCollection<Meeting>("meetings");
        return collection.FindAll()
            .OrderByDescending(m => m.StartTime)
            .ToList();
    }
    
    /// <summary>
    /// Tìm kiếm cuộc họp theo từ khóa (tìm trong tiêu đề, nội dung, kết luận, ghi chú, người chủ trì)
    /// </summary>
    public List<Meeting> SearchMeetings(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return GetAllMeetings();
        
        var kw = keyword.ToLower().Trim();
        var collection = _db.GetCollection<Meeting>("meetings");
        
        return collection.FindAll()
            .Where(m => 
                (m.Title?.ToLower().Contains(kw) ?? false) ||
                (m.Content?.ToLower().Contains(kw) ?? false) ||
                (m.Conclusion?.ToLower().Contains(kw) ?? false) ||
                (m.PersonalNotes?.ToLower().Contains(kw) ?? false) ||
                (m.ChairPerson?.ToLower().Contains(kw) ?? false) ||
                (m.Secretary?.ToLower().Contains(kw) ?? false) ||
                (m.Location?.ToLower().Contains(kw) ?? false) ||
                (m.OrganizingUnit?.ToLower().Contains(kw) ?? false) ||
                (m.MeetingNumber?.ToLower().Contains(kw) ?? false) ||
                (m.Agenda?.ToLower().Contains(kw) ?? false) ||
                (m.Tags?.Any(t => t.ToLower().Contains(kw)) ?? false) ||
                (m.Attendees?.Any(a => a.Name?.ToLower().Contains(kw) ?? false) ?? false) ||
                (m.Tasks?.Any(t => t.Title?.ToLower().Contains(kw) ?? false) ?? false)
            )
            .OrderByDescending(m => m.StartTime)
            .ToList();
    }
    
    /// <summary>
    /// Lọc theo loại cuộc họp
    /// </summary>
    public List<Meeting> GetMeetingsByType(MeetingType type)
    {
        var collection = _db.GetCollection<Meeting>("meetings");
        return collection.Find(m => m.Type == type)
            .OrderByDescending(m => m.StartTime)
            .ToList();
    }
    
    /// <summary>
    /// Lọc theo trạng thái
    /// </summary>
    public List<Meeting> GetMeetingsByStatus(MeetingStatus status)
    {
        var collection = _db.GetCollection<Meeting>("meetings");
        return collection.Find(m => m.Status == status)
            .OrderByDescending(m => m.StartTime)
            .ToList();
    }
    
    /// <summary>
    /// Lọc theo khoảng thời gian
    /// </summary>
    public List<Meeting> GetMeetingsByDateRange(DateTime from, DateTime to)
    {
        var collection = _db.GetCollection<Meeting>("meetings");
        return collection.Find(m => m.StartTime >= from && m.StartTime <= to)
            .OrderByDescending(m => m.StartTime)
            .ToList();
    }
    
    /// <summary>
    /// Lấy cuộc họp sắp tới (N ngày tới)
    /// </summary>
    public List<Meeting> GetUpcomingMeetings(int days = 7)
    {
        var now = DateTime.Now;
        var future = now.AddDays(days);
        var collection = _db.GetCollection<Meeting>("meetings");
        
        return collection.Find(m => m.StartTime >= now && m.StartTime <= future && 
            (m.Status == MeetingStatus.Scheduled || m.Status == MeetingStatus.InProgress))
            .OrderBy(m => m.StartTime)
            .ToList();
    }
    
    /// <summary>
    /// Lấy cuộc họp hôm nay
    /// </summary>
    public List<Meeting> GetTodayMeetings()
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        var collection = _db.GetCollection<Meeting>("meetings");
        
        return collection.Find(m => m.StartTime >= today && m.StartTime < tomorrow)
            .OrderBy(m => m.StartTime)
            .ToList();
    }
    
    /// <summary>
    /// Lấy cuộc họp theo tháng
    /// </summary>
    public List<Meeting> GetMeetingsByMonth(int year, int month)
    {
        var from = new DateTime(year, month, 1);
        var to = from.AddMonths(1).AddTicks(-1);
        return GetMeetingsByDateRange(from, to);
    }
    
    /// <summary>
    /// Lấy cuộc họp theo năm
    /// </summary>
    public List<Meeting> GetMeetingsByYear(int year)
    {
        var from = new DateTime(year, 1, 1);
        var to = new DateTime(year, 12, 31, 23, 59, 59);
        return GetMeetingsByDateRange(from, to);
    }
    
    /// <summary>
    /// Lọc phức hợp (tìm kiếm + loại + trạng thái + khoảng thời gian)
    /// </summary>
    public List<Meeting> FilterMeetings(string? keyword = null, MeetingType? type = null, 
        MeetingStatus? status = null, DateTime? from = null, DateTime? to = null)
    {
        var collection = _db.GetCollection<Meeting>("meetings");
        var query = collection.FindAll().AsEnumerable();
        
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword.ToLower().Trim();
            query = query.Where(m => 
                (m.Title?.ToLower().Contains(kw) ?? false) ||
                (m.Content?.ToLower().Contains(kw) ?? false) ||
                (m.Conclusion?.ToLower().Contains(kw) ?? false) ||
                (m.ChairPerson?.ToLower().Contains(kw) ?? false) ||
                (m.MeetingNumber?.ToLower().Contains(kw) ?? false) ||
                (m.Location?.ToLower().Contains(kw) ?? false));
        }
        
        if (type.HasValue)
            query = query.Where(m => m.Type == type.Value);
        
        if (status.HasValue)
            query = query.Where(m => m.Status == status.Value);
        
        if (from.HasValue)
            query = query.Where(m => m.StartTime >= from.Value);
        
        if (to.HasValue)
            query = query.Where(m => m.StartTime <= to.Value.Date.AddDays(1).AddTicks(-1));
        
        return query.OrderByDescending(m => m.StartTime).ToList();
    }
    
    #endregion
    
    #region Statistics
    
    public int GetTotalMeetings()
    {
        var collection = _db.GetCollection<Meeting>("meetings");
        return collection.Count();
    }
    
    public int GetMeetingsCountThisMonth()
    {
        var from = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        var to = from.AddMonths(1).AddTicks(-1);
        var collection = _db.GetCollection<Meeting>("meetings");
        return collection.Count(m => m.StartTime >= from && m.StartTime <= to);
    }
    
    public int GetMeetingsCountThisYear()
    {
        var year = DateTime.Now.Year;
        var from = new DateTime(year, 1, 1);
        var to = new DateTime(year, 12, 31, 23, 59, 59);
        var collection = _db.GetCollection<Meeting>("meetings");
        return collection.Count(m => m.StartTime >= from && m.StartTime <= to);
    }
    
    public int GetUpcomingCount()
    {
        var now = DateTime.Now;
        var collection = _db.GetCollection<Meeting>("meetings");
        return collection.Count(m => m.StartTime >= now && 
            (m.Status == MeetingStatus.Scheduled || m.Status == MeetingStatus.InProgress));
    }
    
    /// <summary>
    /// Đếm tổng số nhiệm vụ chưa hoàn thành từ tất cả cuộc họp
    /// </summary>
    public int GetPendingTaskCount()
    {
        var collection = _db.GetCollection<Meeting>("meetings");
        return collection.FindAll()
            .SelectMany(m => m.Tasks ?? new List<MeetingTask>())
            .Count(t => t.TaskStatus == MeetingTaskStatus.NotStarted || 
                        t.TaskStatus == MeetingTaskStatus.InProgress);
    }
    
    /// <summary>
    /// Đếm tổng số nhiệm vụ quá hạn
    /// </summary>
    public int GetOverdueTaskCount()
    {
        var now = DateTime.Now;
        var collection = _db.GetCollection<Meeting>("meetings");
        return collection.FindAll()
            .SelectMany(m => m.Tasks ?? new List<MeetingTask>())
            .Count(t => (t.TaskStatus == MeetingTaskStatus.NotStarted || 
                         t.TaskStatus == MeetingTaskStatus.InProgress ||
                         t.TaskStatus == MeetingTaskStatus.Overdue) &&
                        t.Deadline.HasValue && t.Deadline.Value < now);
    }
    
    /// <summary>
    /// Lấy danh sách cuộc họp có nhiệm vụ quá hạn
    /// </summary>
    public List<Meeting> GetMeetingsWithOverdueTasks()
    {
        var now = DateTime.Now;
        var collection = _db.GetCollection<Meeting>("meetings");
        return collection.FindAll()
            .Where(m => m.Tasks != null && m.Tasks.Any(t => 
                (t.TaskStatus == MeetingTaskStatus.NotStarted || 
                 t.TaskStatus == MeetingTaskStatus.InProgress ||
                 t.TaskStatus == MeetingTaskStatus.Overdue) &&
                t.Deadline.HasValue && t.Deadline.Value < now))
            .OrderByDescending(m => m.StartTime)
            .ToList();
    }
    
    /// <summary>
    /// Thống kê theo loại cuộc họp
    /// </summary>
    public Dictionary<MeetingType, int> GetStatsByType()
    {
        var collection = _db.GetCollection<Meeting>("meetings");
        return collection.FindAll()
            .GroupBy(m => m.Type)
            .ToDictionary(g => g.Key, g => g.Count());
    }
    
    /// <summary>
    /// Thống kê theo tháng trong năm
    /// </summary>
    public Dictionary<int, int> GetStatsByMonth(int year)
    {
        var collection = _db.GetCollection<Meeting>("meetings");
        var meetings = collection.Find(m => m.StartTime.Year == year);
        
        var result = new Dictionary<int, int>();
        for (int i = 1; i <= 12; i++)
            result[i] = 0;
        
        foreach (var m in meetings)
            result[m.StartTime.Month]++;
        
        return result;
    }
    
    #endregion
    
    #region Task Management
    
    /// <summary>
    /// Cập nhật trạng thái nhiệm vụ trong cuộc họp
    /// </summary>
    public bool UpdateTaskStatus(string meetingId, string taskId, MeetingTaskStatus newStatus)
    {
        var meeting = GetMeetingById(meetingId);
        if (meeting == null) return false;
        
        var task = meeting.Tasks?.FirstOrDefault(t => t.Id == taskId);
        if (task == null) return false;
        
        task.TaskStatus = newStatus;
        if (newStatus == MeetingTaskStatus.Completed)
            task.CompletionDate = DateTime.Now;
        
        return UpdateMeeting(meeting);
    }
    
    /// <summary>
    /// Tự động cập nhật trạng thái quá hạn cho các nhiệm vụ
    /// </summary>
    private void AutoUpdateOverdueTasks(Meeting meeting)
    {
        if (meeting.Tasks == null) return;
        
        var now = DateTime.Now;
        foreach (var task in meeting.Tasks)
        {
            if ((task.TaskStatus == MeetingTaskStatus.NotStarted || 
                 task.TaskStatus == MeetingTaskStatus.InProgress) &&
                task.Deadline.HasValue && task.Deadline.Value < now)
            {
                task.TaskStatus = MeetingTaskStatus.Overdue;
            }
        }
    }
    
    /// <summary>
    /// Chạy cập nhật overdue cho tất cả cuộc họp (gọi khi app khởi động)
    /// </summary>
    public void RefreshOverdueTasks()
    {
        var collection = _db.GetCollection<Meeting>("meetings");
        var now = DateTime.Now;
        
        var meetings = collection.FindAll()
            .Where(m => m.Tasks != null && m.Tasks.Any(t => 
                (t.TaskStatus == MeetingTaskStatus.NotStarted || 
                 t.TaskStatus == MeetingTaskStatus.InProgress) &&
                t.Deadline.HasValue && t.Deadline.Value < now))
            .ToList();
        
        foreach (var meeting in meetings)
        {
            AutoUpdateOverdueTasks(meeting);
            collection.Update(meeting);
        }
    }
    
    #endregion
    
    public void Dispose()
    {
        _db?.Dispose();
    }
}
