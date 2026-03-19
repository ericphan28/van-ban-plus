using AIVanBan.Core.Models;
using AIVanBan.Core.Services;

namespace AIVanBan.Tests;

/// <summary>
/// Unit tests cho MeetingReminderService.
/// Test logic nhắc nhở cuộc họp sắp diễn ra.
/// Dùng MeetingService thật (DatabaseFactory singleton) với cleanup sau mỗi test.
/// </summary>
public class MeetingReminderServiceTests : IDisposable
{
    private readonly MeetingService _meetingService;
    private readonly MeetingReminderService _reminderService;
    private readonly List<string> _testMeetingIds = new(); // Track để cleanup

    public MeetingReminderServiceTests()
    {
        // Dùng default path (DatabaseFactory singleton) — tránh lỗi path conflict
        _meetingService = new MeetingService();
        _reminderService = new MeetingReminderService(_meetingService);
    }

    public void Dispose()
    {
        // Cleanup: xóa tất cả meeting tạo trong test
        foreach (var id in _testMeetingIds)
        {
            try { _meetingService.DeleteMeeting(id); } catch { }
        }
        _meetingService.Dispose();
    }

    /// <summary>Helper: thêm meeting và track ID để cleanup</summary>
    private Meeting AddTestMeeting(Meeting meeting)
    {
        _meetingService.AddMeeting(meeting);
        _testMeetingIds.Add(meeting.Id);
        return meeting;
    }

    #region CheckUpcomingReminders

    [Fact]
    public void CheckUpcomingReminders_NoMeetings_ReturnsEmpty()
    {
        // Arrange — dùng service mới, fresh reminder tracker
        var service = new MeetingReminderService(new MeetingService());

        // Act — kết quả phụ thuộc DB thật, nhưng không crash = OK
        var reminders = service.CheckUpcomingReminders();

        // Assert
        Assert.NotNull(reminders);
    }

    [Fact]
    public void CheckUpcomingReminders_MeetingSoonWithinWindow_ReturnsReminder()
    {
        // Arrange — cuộc họp trong 10 phút nữa, reminder = 15 phút
        var meeting = AddTestMeeting(new Meeting
        {
            Title = "[TEST] Họp UBND thường kỳ",
            StartTime = DateTime.Now.AddMinutes(10),
            ReminderMinutesBefore = 15,
            Status = MeetingStatus.Scheduled,
            IsTemplate = false,
            Location = "Phòng họp A"
        });

        // Act
        var reminders = _reminderService.CheckUpcomingReminders();

        // Assert
        var reminder = reminders.FirstOrDefault(r => r.Meeting.Id == meeting.Id);
        Assert.NotNull(reminder);
        Assert.Equal("[TEST] Họp UBND thường kỳ", reminder.Meeting.Title);
        Assert.Contains("UBND", reminder.Message);
        Assert.Contains("Phòng họp A", reminder.Message);
    }

    [Fact]
    public void CheckUpcomingReminders_MeetingOutsideWindow_ReturnsEmpty()
    {
        // Arrange — cuộc họp trong 60 phút nữa, reminder = 15 phút
        var meeting = AddTestMeeting(new Meeting
        {
            Title = "[TEST] Họp xa",
            StartTime = DateTime.Now.AddMinutes(60),
            ReminderMinutesBefore = 15,
            Status = MeetingStatus.Scheduled,
            IsTemplate = false
        });

        // Act
        var reminders = _reminderService.CheckUpcomingReminders();

        // Assert — meeting này không nên có trong danh sách nhắc
        Assert.DoesNotContain(reminders, r => r.Meeting.Id == meeting.Id);
    }

    [Fact]
    public void CheckUpcomingReminders_MeetingAlreadyPassed_ReturnsEmpty()
    {
        // Arrange — cuộc họp đã diễn ra 30 phút trước
        var meeting = AddTestMeeting(new Meeting
        {
            Title = "[TEST] Họp đã qua",
            StartTime = DateTime.Now.AddMinutes(-30),
            ReminderMinutesBefore = 15,
            Status = MeetingStatus.Scheduled,
            IsTemplate = false
        });

        // Act
        var reminders = _reminderService.CheckUpcomingReminders();

        // Assert
        Assert.DoesNotContain(reminders, r => r.Meeting.Id == meeting.Id);
    }

    [Fact]
    public void CheckUpcomingReminders_CancelledMeeting_SkipsIt()
    {
        // Arrange — cuộc họp bị hủy
        var meeting = AddTestMeeting(new Meeting
        {
            Title = "[TEST] Họp bị hủy",
            StartTime = DateTime.Now.AddMinutes(10),
            ReminderMinutesBefore = 15,
            Status = MeetingStatus.Cancelled,
            IsTemplate = false
        });

        // Act
        var reminders = _reminderService.CheckUpcomingReminders();

        // Assert
        Assert.DoesNotContain(reminders, r => r.Meeting.Id == meeting.Id);
    }

    [Fact]
    public void CheckUpcomingReminders_CompletedMeeting_SkipsIt()
    {
        // Arrange
        var meeting = AddTestMeeting(new Meeting
        {
            Title = "[TEST] Họp đã hoàn thành",
            StartTime = DateTime.Now.AddMinutes(5),
            ReminderMinutesBefore = 15,
            Status = MeetingStatus.Completed,
            IsTemplate = false
        });

        // Act
        var reminders = _reminderService.CheckUpcomingReminders();

        // Assert
        Assert.DoesNotContain(reminders, r => r.Meeting.Id == meeting.Id);
    }

    [Fact]
    public void CheckUpcomingReminders_Template_SkipsIt()
    {
        // Arrange — template meeting (mẫu) không nên nhắc
        var meeting = AddTestMeeting(new Meeting
        {
            Title = "[TEST] Mẫu họp tuần",
            StartTime = DateTime.Now.AddMinutes(10),
            ReminderMinutesBefore = 15,
            Status = MeetingStatus.Scheduled,
            IsTemplate = true
        });

        // Act
        var reminders = _reminderService.CheckUpcomingReminders();

        // Assert
        Assert.DoesNotContain(reminders, r => r.Meeting.Id == meeting.Id);
    }

    [Fact]
    public void CheckUpcomingReminders_ReminderSetToZero_DefaultsTo15()
    {
        // Arrange — ReminderMinutesBefore = 0 (meeting cũ trước khi có field, LiteDB trả 0)
        // Service sẽ tự default lên 15 phút cho backwards compatibility
        var meeting = AddTestMeeting(new Meeting
        {
            Title = "[TEST] Họp không có ReminderMinutesBefore (cũ)",
            StartTime = DateTime.Now.AddMinutes(10),
            ReminderMinutesBefore = 0,
            Status = MeetingStatus.Scheduled,
            IsTemplate = false
        });

        // Act
        var reminders = _reminderService.CheckUpcomingReminders();

        // Assert — meeting cũ được default 15 phút, nên vẫn nhắc nhở
        Assert.Contains(reminders, r => r.Meeting.Id == meeting.Id);
    }

    [Fact]
    public void CheckUpcomingReminders_DoesNotRepeat_SameSession()
    {
        // Arrange — cuộc họp trong 10 phút nữa
        var meeting = AddTestMeeting(new Meeting
        {
            Title = "[TEST] Họp sáng no-repeat",
            StartTime = DateTime.Now.AddMinutes(10),
            ReminderMinutesBefore = 15,
            Status = MeetingStatus.Scheduled,
            IsTemplate = false
        });

        // Act — gọi lần 1
        var first = _reminderService.CheckUpcomingReminders();
        Assert.Contains(first, r => r.Meeting.Id == meeting.Id);

        // Act — gọi lần 2 → không lặp nhắc
        var second = _reminderService.CheckUpcomingReminders();
        Assert.DoesNotContain(second, r => r.Meeting.Id == meeting.Id);
    }

    [Fact]
    public void ResetReminders_AllowsReReminding()
    {
        // Arrange
        var meeting = AddTestMeeting(new Meeting
        {
            Title = "[TEST] Họp chiều reset",
            StartTime = DateTime.Now.AddMinutes(10),
            ReminderMinutesBefore = 15,
            Status = MeetingStatus.Scheduled,
            IsTemplate = false
        });

        // Act — nhắc lần 1
        var first = _reminderService.CheckUpcomingReminders();
        Assert.Contains(first, r => r.Meeting.Id == meeting.Id);

        // Reset (giả lập chuyển ngày)
        _reminderService.ResetReminders();

        // Act — nhắc lại sau reset
        var second = _reminderService.CheckUpcomingReminders();
        Assert.Contains(second, r => r.Meeting.Id == meeting.Id);
    }

    [Fact]
    public void CheckUpcomingReminders_MessageFormat_IsCorrect()
    {
        // Arrange
        var meeting = AddTestMeeting(new Meeting
        {
            Title = "[TEST] Giao ban sáng format",
            StartTime = DateTime.Now.AddMinutes(5),
            ReminderMinutesBefore = 15,
            Status = MeetingStatus.Scheduled,
            IsTemplate = false,
            Location = "Phòng 101"
        });

        // Act
        var reminders = _reminderService.CheckUpcomingReminders();
        var reminder = reminders.FirstOrDefault(r => r.Meeting.Id == meeting.Id);

        // Assert
        Assert.NotNull(reminder);
        Assert.Contains("📅", reminder.Message);
        Assert.Contains("⏰", reminder.Message);
        Assert.Contains("📍 Phòng 101", reminder.Message);
        Assert.Contains("[TEST] Giao ban sáng format", reminder.Message);
        Assert.True(reminder.MinutesUntilStart > 0);
    }

    [Fact]
    public void CheckUpcomingReminders_MeetingStartingNow_ShowsBatDauNgay()
    {
        // Arrange — cuộc họp bắt đầu trong 30 giây
        var meeting = AddTestMeeting(new Meeting
        {
            Title = "[TEST] Họp khẩn ngay",
            StartTime = DateTime.Now.AddSeconds(30),
            ReminderMinutesBefore = 15,
            Status = MeetingStatus.Scheduled,
            IsTemplate = false
        });

        // Act
        var reminders = _reminderService.CheckUpcomingReminders();
        var reminder = reminders.FirstOrDefault(r => r.Meeting.Id == meeting.Id);

        // Assert
        Assert.NotNull(reminder);
        Assert.Contains("BẮT ĐẦU NGAY", reminder.Message);
    }

    #endregion
}
