using AIVanBan.Core.Models;

namespace AIVanBan.Core.Services;

/// <summary>
/// Thông tin nhắc nhở cuộc họp sắp diễn ra
/// </summary>
public class MeetingReminder
{
    public Meeting Meeting { get; set; } = null!;
    public int MinutesUntilStart { get; set; }
    public string Message { get; set; } = "";
}

/// <summary>
/// Service kiểm tra và tạo nhắc nhở cho cuộc họp sắp diễn ra.
/// Được gọi định kỳ bởi DispatcherTimer trong MainWindow.
/// </summary>
public class MeetingReminderService
{
    private readonly MeetingService _meetingService;
    
    // Track meetings đã nhắc (tránh nhắc lặp trong cùng session)
    private readonly HashSet<string> _remindedMeetingIds = new();

    public MeetingReminderService(MeetingService meetingService)
    {
        _meetingService = meetingService;
    }

    /// <summary>
    /// Kiểm tra cuộc họp sắp diễn ra cần nhắc nhở.
    /// Trả về danh sách cuộc họp cần hiển thị thông báo.
    /// </summary>
    public List<MeetingReminder> CheckUpcomingReminders()
    {
        var reminders = new List<MeetingReminder>();
        
        try
        {
            var now = DateTime.Now;
            // Lấy cuộc họp trong 2 giờ tới
            var upcoming = _meetingService.GetMeetingsByDateRange(now.Date, now.Date.AddDays(1))
                .Where(m => m.Status != MeetingStatus.Cancelled 
                         && m.Status != MeetingStatus.Completed
                         && !m.IsTemplate
                         && m.ReminderMinutesBefore > 0
                         && m.StartTime > now) // Chưa bắt đầu
                .ToList();

            foreach (var meeting in upcoming)
            {
                var minutesUntil = (int)(meeting.StartTime - now).TotalMinutes;
                
                // Chỉ nhắc khi đúng trong khoảng reminder (± 2 phút buffer)
                if (minutesUntil <= meeting.ReminderMinutesBefore && minutesUntil >= -2)
                {
                    // Bỏ qua nếu đã nhắc trong session này
                    if (_remindedMeetingIds.Contains(meeting.Id)) continue;
                    
                    _remindedMeetingIds.Add(meeting.Id);
                    
                    var timeText = minutesUntil <= 1 
                        ? "BẮT ĐẦU NGAY" 
                        : $"còn {minutesUntil} phút";
                    
                    reminders.Add(new MeetingReminder
                    {
                        Meeting = meeting,
                        MinutesUntilStart = minutesUntil,
                        Message = $"📅 {meeting.Title}\n⏰ {meeting.StartTime:HH:mm} ({timeText})" +
                                 (string.IsNullOrEmpty(meeting.Location) ? "" : $"\n📍 {meeting.Location}")
                    });
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ MeetingReminderService error: {ex.Message}");
        }
        
        return reminders;
    }

    /// <summary>
    /// Reset danh sách đã nhắc (gọi khi chuyển ngày)
    /// </summary>
    public void ResetReminders()
    {
        _remindedMeetingIds.Clear();
    }
}
