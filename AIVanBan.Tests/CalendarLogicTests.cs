namespace AIVanBan.Tests;

/// <summary>
/// Unit tests cho Calendar logic.
/// Test các hàm tính toán lịch, không cần UI.
/// </summary>
public class CalendarLogicTests
{
    #region GetMondayOfWeek Logic

    /// <summary>
    /// Replicate logic từ CalendarPage.GetMondayOfWeek (static helper)
    /// </summary>
    private static DateTime GetMondayOfWeek(DateTime date)
    {
        int diff = ((int)date.DayOfWeek + 6) % 7; // Mon=0
        return date.Date.AddDays(-diff);
    }

    [Fact]
    public void GetMondayOfWeek_Monday_ReturnsSameDay()
    {
        var monday = new DateTime(2026, 3, 2); // Monday
        Assert.Equal(DayOfWeek.Monday, monday.DayOfWeek);
        Assert.Equal(monday, GetMondayOfWeek(monday));
    }

    [Fact]
    public void GetMondayOfWeek_Wednesday_ReturnsPreviousMonday()
    {
        var wednesday = new DateTime(2026, 3, 4); // Wednesday
        Assert.Equal(DayOfWeek.Wednesday, wednesday.DayOfWeek);
        
        var result = GetMondayOfWeek(wednesday);
        Assert.Equal(new DateTime(2026, 3, 2), result);
        Assert.Equal(DayOfWeek.Monday, result.DayOfWeek);
    }

    [Fact]
    public void GetMondayOfWeek_Sunday_ReturnsPreviousMonday()
    {
        var sunday = new DateTime(2026, 3, 8); // Sunday
        Assert.Equal(DayOfWeek.Sunday, sunday.DayOfWeek);
        
        var result = GetMondayOfWeek(sunday);
        Assert.Equal(new DateTime(2026, 3, 2), result);
        Assert.Equal(DayOfWeek.Monday, result.DayOfWeek);
    }

    [Fact]
    public void GetMondayOfWeek_Saturday_ReturnsPreviousMonday()
    {
        var saturday = new DateTime(2026, 3, 7); // Saturday
        Assert.Equal(DayOfWeek.Saturday, saturday.DayOfWeek);
        
        var result = GetMondayOfWeek(saturday);
        Assert.Equal(new DateTime(2026, 3, 2), result);
    }

    [Fact]
    public void GetMondayOfWeek_CrossMonthBoundary()
    {
        // Wednesday March 4 → Monday March 2 (crosses nothing)
        // But Thursday Jan 1, 2026 → Monday Dec 29, 2025 (crosses year!)
        var jan1 = new DateTime(2026, 1, 1); // Thursday
        Assert.Equal(DayOfWeek.Thursday, jan1.DayOfWeek);
        
        var result = GetMondayOfWeek(jan1);
        Assert.Equal(new DateTime(2025, 12, 29), result);
        Assert.Equal(DayOfWeek.Monday, result.DayOfWeek);
    }

    [Fact]
    public void GetMondayOfWeek_StripsTimeComponent()
    {
        var dateWithTime = new DateTime(2026, 3, 4, 14, 30, 0); // Wed 14:30
        var result = GetMondayOfWeek(dateWithTime);
        
        Assert.Equal(TimeSpan.Zero, result.TimeOfDay); // Thời gian phải = 00:00:00
    }

    #endregion

    #region Week Navigation Logic

    [Fact]
    public void WeekNavigation_PrevWeek_Subtracts7Days()
    {
        var currentWeekStart = new DateTime(2026, 3, 2); // Monday
        var prevWeek = currentWeekStart.AddDays(-7);
        
        Assert.Equal(new DateTime(2026, 2, 23), prevWeek);
        Assert.Equal(DayOfWeek.Monday, prevWeek.DayOfWeek);
    }

    [Fact]
    public void WeekNavigation_NextWeek_Adds7Days()
    {
        var currentWeekStart = new DateTime(2026, 3, 2); // Monday
        var nextWeek = currentWeekStart.AddDays(7);
        
        Assert.Equal(new DateTime(2026, 3, 9), nextWeek);
        Assert.Equal(DayOfWeek.Monday, nextWeek.DayOfWeek);
    }

    [Fact]
    public void WeekNavigation_4WeeksForward_Equals28Days()
    {
        var start = new DateTime(2026, 3, 2);
        var fourWeeksLater = start.AddDays(28);
        
        Assert.Equal(new DateTime(2026, 3, 30), fourWeeksLater);
    }

    #endregion

    #region Month Grid Logic

    [Fact]
    public void MonthGrid_FirstDayOffset_Monday_IsZero()
    {
        // March 2026 starts on Sunday
        var firstDay = new DateTime(2026, 3, 1);
        int startOffset = ((int)firstDay.DayOfWeek + 6) % 7;
        
        Assert.Equal(DayOfWeek.Sunday, firstDay.DayOfWeek);
        Assert.Equal(6, startOffset); // Sunday → offset 6 (Mon=0 based)
    }

    [Fact]
    public void MonthGrid_FirstDayOffset_MondayIsZero()
    {
        // June 2026 starts on Monday
        var firstDay = new DateTime(2026, 6, 1);
        int startOffset = ((int)firstDay.DayOfWeek + 6) % 7;
        
        Assert.Equal(DayOfWeek.Monday, firstDay.DayOfWeek);
        Assert.Equal(0, startOffset); // Monday → offset 0
    }

    [Fact]
    public void MonthGrid_TotalCells_Is42()
    {
        // Luôn hiển thị 6 hàng × 7 cột = 42 ô
        var firstDay = new DateTime(2026, 3, 1);
        int startOffset = ((int)firstDay.DayOfWeek + 6) % 7;
        int daysInMonth = DateTime.DaysInMonth(2026, 3);
        int totalFilled = startOffset + daysInMonth;
        int remaining = 42 - totalFilled;
        
        Assert.Equal(42, startOffset + daysInMonth + remaining);
        Assert.True(remaining >= 0, "Remaining cells should be non-negative");
    }

    #endregion

    #region Time Grid Logic (Weekly View)

    [Fact]
    public void TimeGrid_HourRange_7to18_Has12Rows()
    {
        int startHour = 7, endHour = 18;
        int rows = endHour - startHour + 1;
        
        Assert.Equal(12, rows); // 7,8,9,10,11,12,13,14,15,16,17,18
    }

    [Fact]
    public void TimeGrid_MeetingAtRow_CalculatesCorrectly()
    {
        int startHour = 7;
        
        // Meeting at 9:00 → row 2 (9 - 7 = 2)
        Assert.Equal(2, 9 - startHour);
        
        // Meeting at 7:00 → row 0
        Assert.Equal(0, 7 - startHour);
        
        // Meeting at 18:00 → row 11
        Assert.Equal(11, 18 - startHour);
    }

    [Fact]
    public void TimeGrid_MeetingSpan_CalculatesCorrectly()
    {
        int startHour = 7;
        int endHour = 18;
        
        // Meeting 9:00-11:00 → startRow=2, endRow=4, span=2
        int meetStart = 9, meetEnd = 11;
        int startRow = Math.Max(0, meetStart - startHour);
        int endRow = Math.Min(endHour - startHour, meetEnd - startHour);
        int span = Math.Max(1, endRow - startRow);
        
        Assert.Equal(2, startRow);
        Assert.Equal(4, endRow);
        Assert.Equal(2, span);
    }

    [Fact]
    public void TimeGrid_MeetingBefore7_ClampsToRow0()
    {
        int startHour = 7;
        
        // Meeting at 6:00 (trước giờ hiển thị) → clamp to row 0
        int meetStart = 6;
        int startRow = Math.Max(0, meetStart - startHour);
        
        Assert.Equal(0, startRow);
    }

    [Fact]
    public void TimeGrid_MeetingAfter18_ClampsToLastRow()
    {
        int startHour = 7, endHour = 18;
        
        // Meeting ending at 20:00 → clamp to row 11
        int meetEnd = 20;
        int endRow = Math.Min(endHour - startHour, meetEnd - startHour);
        
        Assert.Equal(11, endRow);
    }

    #endregion

    #region TruncateText Logic

    private static string TruncateText(string text, int maxLen)
    {
        if (string.IsNullOrEmpty(text)) return "";
        return text.Length <= maxLen ? text : text.Substring(0, maxLen) + "…";
    }

    [Theory]
    [InlineData("", 10, "")]
    [InlineData(null, 10, "")]
    [InlineData("Hello", 10, "Hello")]
    [InlineData("Hello World", 5, "Hello…")]
    [InlineData("Họp UBND thường kỳ tháng 3/2026", 16, "Họp UBND thường …")]
    public void TruncateText_Works(string? input, int maxLen, string expected)
    {
        Assert.Equal(expected, TruncateText(input!, maxLen));
    }

    #endregion

    #region RelativeTime Logic

    private static string GetRelativeTimeText(DateTime meetingTime)
    {
        var now = DateTime.Now;
        var diff = meetingTime - now;
        
        if (diff.TotalMinutes < 0 && diff.TotalMinutes > -60)
            return "Vừa qua";
        if (diff.TotalMinutes < 0)
        {
            if (diff.TotalHours > -24) return $"{Math.Abs((int)diff.TotalHours)} giờ trước";
            return $"{Math.Abs((int)diff.TotalDays)} ngày trước";
        }
        
        if (diff.TotalMinutes < 30) return $"Sau {(int)diff.TotalMinutes} phút";
        if (diff.TotalHours < 1) return "Sau 30 phút";
        if (diff.TotalHours < 24) return $"Sau {(int)diff.TotalHours} giờ";
        if (diff.TotalDays < 2) return "Ngày mai";
        return $"Còn {(int)diff.TotalDays} ngày";
    }

    [Fact]
    public void RelativeTime_FutureMinutes()
    {
        var text = GetRelativeTimeText(DateTime.Now.AddMinutes(10));
        Assert.Contains("Sau", text);
        Assert.Contains("phút", text);
    }

    [Fact]
    public void RelativeTime_FutureHours()
    {
        var text = GetRelativeTimeText(DateTime.Now.AddHours(3));
        Assert.Contains("Sau", text);
        Assert.Contains("giờ", text);
    }

    [Fact]
    public void RelativeTime_Tomorrow()
    {
        var text = GetRelativeTimeText(DateTime.Now.AddHours(30));
        Assert.Equal("Ngày mai", text);
    }

    [Fact]
    public void RelativeTime_MultipleDays()
    {
        var text = GetRelativeTimeText(DateTime.Now.AddDays(5));
        Assert.StartsWith("Còn", text);
        Assert.Contains("ngày", text);
    }

    [Fact]
    public void RelativeTime_JustPassed()
    {
        var text = GetRelativeTimeText(DateTime.Now.AddMinutes(-10));
        Assert.Equal("Vừa qua", text);
    }

    #endregion
}
