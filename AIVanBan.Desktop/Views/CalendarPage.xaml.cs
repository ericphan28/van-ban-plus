using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AIVanBan.Core.Models;
using AIVanBan.Core.Services;

namespace AIVanBan.Desktop.Views;

/// <summary>
/// Lịch tổng hợp — gom deadline VB, cuộc họp, nhiệm vụ họp vào 1 view lịch tháng.
/// Color-code: 🔴 quá hạn, 🟡 sắp hạn, 🔵 họp, 🟢 task.
/// </summary>
public partial class CalendarPage : Page
{
    private readonly DocumentService _documentService;
    private readonly MeetingService _meetingService;
    
    private DateTime _currentMonth;
    private DateTime? _selectedDate;

    // Event data for current month
    private Dictionary<DateTime, List<CalendarEvent>> _monthEvents = new();

    public CalendarPage(DocumentService documentService)
    {
        InitializeComponent();
        _documentService = documentService;
        _meetingService = new MeetingService();
        _currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        
        Loaded += (s, e) => RenderCalendar();
    }

    #region Navigation

    private void PrevMonth_Click(object sender, RoutedEventArgs e)
    {
        _currentMonth = _currentMonth.AddMonths(-1);
        RenderCalendar();
    }

    private void NextMonth_Click(object sender, RoutedEventArgs e)
    {
        _currentMonth = _currentMonth.AddMonths(1);
        RenderCalendar();
    }

    private void Today_Click(object sender, RoutedEventArgs e)
    {
        _currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        _selectedDate = DateTime.Today;
        RenderCalendar();
        ShowDayDetail(DateTime.Today);
    }

    #endregion

    #region Render Calendar

    private void RenderCalendar()
    {
        txtMonthYear.Text = $"Tháng {_currentMonth.Month:D2}/{_currentMonth.Year}";

        // Load events for the month
        LoadMonthEvents();

        // Clear and rebuild grid
        calendarGrid.Children.Clear();

        // Calculate first day offset (Monday = 0)
        var firstDay = _currentMonth;
        int startOffset = ((int)firstDay.DayOfWeek + 6) % 7; // Mon=0, Tue=1, ..., Sun=6
        int daysInMonth = DateTime.DaysInMonth(_currentMonth.Year, _currentMonth.Month);
        var today = DateTime.Today;

        // Fill days from previous month (dimmed)
        var prevMonth = _currentMonth.AddMonths(-1);
        int prevDays = DateTime.DaysInMonth(prevMonth.Year, prevMonth.Month);
        for (int i = 0; i < startOffset; i++)
        {
            int day = prevDays - startOffset + 1 + i;
            var date = new DateTime(prevMonth.Year, prevMonth.Month, day);
            calendarGrid.Children.Add(CreateDayCell(date, isCurrentMonth: false));
        }

        // Fill current month days
        for (int day = 1; day <= daysInMonth; day++)
        {
            var date = new DateTime(_currentMonth.Year, _currentMonth.Month, day);
            calendarGrid.Children.Add(CreateDayCell(date, isCurrentMonth: true));
        }

        // Fill remaining cells with next month
        int totalCells = startOffset + daysInMonth;
        int remaining = 42 - totalCells; // 6 rows × 7 cols
        var nextMonth = _currentMonth.AddMonths(1);
        for (int i = 1; i <= remaining; i++)
        {
            var date = new DateTime(nextMonth.Year, nextMonth.Month, i);
            calendarGrid.Children.Add(CreateDayCell(date, isCurrentMonth: false));
        }
    }

    private Border CreateDayCell(DateTime date, bool isCurrentMonth)
    {
        var today = DateTime.Today;
        bool isToday = date.Date == today;
        bool isSelected = _selectedDate.HasValue && date.Date == _selectedDate.Value.Date;
        bool isWeekend = date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
        
        var events = _monthEvents.ContainsKey(date.Date) ? _monthEvents[date.Date] : new List<CalendarEvent>();

        // Container
        var border = new Border
        {
            BorderBrush = new SolidColorBrush(isSelected ? Color.FromRgb(25, 118, 210) : Color.FromRgb(224, 224, 224)),
            BorderThickness = new Thickness(isSelected ? 2 : 0.5),
            Margin = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Background = isToday
                ? new SolidColorBrush(Color.FromRgb(227, 242, 253)) // light blue
                : isSelected
                    ? new SolidColorBrush(Color.FromRgb(232, 245, 253))
                    : Brushes.White,
            Cursor = Cursors.Hand,
            Tag = date
        };

        border.MouseLeftButtonDown += DayCell_Click;

        var stack = new StackPanel { Margin = new Thickness(4, 2, 4, 2) };

        // Day number
        var dayText = new TextBlock
        {
            Text = date.Day.ToString(),
            FontSize = 13,
            FontWeight = isToday ? FontWeights.Bold : FontWeights.Normal,
            Foreground = !isCurrentMonth
                ? new SolidColorBrush(Color.FromRgb(189, 189, 189))
                : isToday
                    ? new SolidColorBrush(Color.FromRgb(21, 101, 192))
                    : isWeekend
                        ? new SolidColorBrush(Color.FromRgb(198, 40, 40))
                        : new SolidColorBrush(Color.FromRgb(55, 71, 79)),
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 0, 2, 0)
        };
        stack.Children.Add(dayText);

        // Event indicators (max 3 visible, then "+N")
        int shown = 0;
        foreach (var evt in events.Take(3))
        {
            var indicator = new Border
            {
                Background = new SolidColorBrush(evt.Color),
                CornerRadius = new CornerRadius(2),
                Padding = new Thickness(3, 1, 3, 1),
                Margin = new Thickness(0, 1, 0, 0)
            };
            var label = new TextBlock
            {
                Text = evt.ShortLabel,
                FontSize = 9,
                Foreground = Brushes.White,
                TextTrimming = TextTrimming.CharacterEllipsis,
                MaxWidth = 120
            };
            indicator.Child = label;
            stack.Children.Add(indicator);
            shown++;
        }

        if (events.Count > 3)
        {
            var moreText = new TextBlock
            {
                Text = $"+{events.Count - 3} khác",
                FontSize = 9,
                Foreground = new SolidColorBrush(Color.FromRgb(136, 136, 136)),
                Margin = new Thickness(2, 1, 0, 0)
            };
            stack.Children.Add(moreText);
        }

        border.Child = stack;
        return border;
    }

    private void DayCell_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is Border border && border.Tag is DateTime date)
        {
            _selectedDate = date;

            // If clicked date is in different month, navigate there
            if (date.Month != _currentMonth.Month || date.Year != _currentMonth.Year)
            {
                _currentMonth = new DateTime(date.Year, date.Month, 1);
            }

            RenderCalendar();
            ShowDayDetail(date);
        }
    }

    #endregion

    #region Load Events

    private void LoadMonthEvents()
    {
        _monthEvents.Clear();

        // Expand range to include prev/next month days visible in grid
        var rangeStart = _currentMonth.AddDays(-7);
        var rangeEnd = _currentMonth.AddMonths(1).AddDays(7);

        // === 1. VB có hạn xử lý (DueDate) ===
        var allDocs = _documentService.GetAllDocuments();
        var docsWithDue = allDocs.Where(d =>
            d.DueDate.HasValue &&
            d.DueDate.Value.Date >= rangeStart.Date &&
            d.DueDate.Value.Date <= rangeEnd.Date &&
            !d.IsDeleted
        ).ToList();

        foreach (var doc in docsWithDue)
        {
            var dueDate = doc.DueDate!.Value.Date;
            var isOverdue = dueDate < DateTime.Today
                && doc.WorkflowStatus != DocumentStatus.Archived
                && doc.WorkflowStatus != DocumentStatus.Published;
            var isDueSoon = !isOverdue && dueDate <= DateTime.Today.AddDays(3)
                && doc.WorkflowStatus != DocumentStatus.Archived
                && doc.WorkflowStatus != DocumentStatus.Published;

            var evt = new CalendarEvent
            {
                Type = isOverdue ? EventType.Overdue : isDueSoon ? EventType.DueSoon : EventType.Document,
                ShortLabel = $"📄 {TruncateText(doc.Number, 12)}",
                FullLabel = $"{doc.Number} — {doc.Title}",
                Detail = $"Hạn: {doc.DueDate:dd/MM/yyyy}\nLoại: {doc.Type.GetDisplayName()}\nCơ quan: {doc.Issuer}",
                Color = isOverdue
                    ? Color.FromRgb(198, 40, 40)   // Red
                    : isDueSoon
                        ? Color.FromRgb(245, 127, 23) // Orange
                        : Color.FromRgb(100, 181, 246) // Light blue
            };

            AddEvent(dueDate, evt);
        }

        // === 2. Cuộc họp ===
        try
        {
            var meetings = _meetingService.GetMeetingsByDateRange(rangeStart, rangeEnd);
            foreach (var meeting in meetings)
            {
                var meetDate = meeting.StartTime.Date;
                var evt = new CalendarEvent
                {
                    Type = EventType.Meeting,
                    ShortLabel = $"🔵 {TruncateText(meeting.Title, 12)}",
                    FullLabel = meeting.Title,
                    Detail = $"Thời gian: {meeting.StartTime:HH:mm} - {meeting.EndTime:HH:mm}\n" +
                             $"Địa điểm: {meeting.Location}\n" +
                             $"Chủ trì: {meeting.ChairPerson}\n" +
                             $"Trạng thái: {MeetingHelper.GetStatusName(meeting.Status)}",
                    Color = Color.FromRgb(21, 101, 192) // Blue
                };
                AddEvent(meetDate, evt);

                // Also add meeting tasks with deadlines
                if (meeting.Tasks != null)
                {
                    foreach (var task in meeting.Tasks.Where(t => t.Deadline.HasValue && t.Deadline.Value.Date >= rangeStart.Date && t.Deadline.Value.Date <= rangeEnd.Date))
                    {
                        bool isTaskDone = task.TaskStatus == MeetingTaskStatus.Completed;
                        bool isTaskOverdue = task.Deadline!.Value.Date < DateTime.Today && !isTaskDone;

                        var taskEvt = new CalendarEvent
                        {
                            Type = isTaskDone ? EventType.TaskDone : isTaskOverdue ? EventType.Overdue : EventType.Task,
                            ShortLabel = isTaskDone ? $"✅ {TruncateText(task.Title, 11)}" : $"📋 {TruncateText(task.Title, 11)}",
                            FullLabel = task.Title,
                            Detail = $"Từ họp: {meeting.Title}\nGiao: {task.AssignedTo}\nHạn: {task.Deadline:dd/MM/yyyy}\n" +
                                     $"TT: {MeetingHelper.GetTaskStatusName(task.TaskStatus)}",
                            Color = isTaskDone
                                ? Color.FromRgb(46, 125, 50)    // Green
                                : isTaskOverdue
                                    ? Color.FromRgb(198, 40, 40) // Red
                                    : Color.FromRgb(56, 142, 60) // Green
                        };
                        AddEvent(task.Deadline.Value.Date, taskEvt);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Calendar: Error loading meetings: {ex.Message}");
        }
    }

    private void AddEvent(DateTime date, CalendarEvent evt)
    {
        if (!_monthEvents.ContainsKey(date))
            _monthEvents[date] = new List<CalendarEvent>();
        _monthEvents[date].Add(evt);
    }

    #endregion

    #region Day Detail Panel

    private void ShowDayDetail(DateTime date)
    {
        var vietnameseDays = new[] { "Chủ nhật", "Thứ 2", "Thứ 3", "Thứ 4", "Thứ 5", "Thứ 6", "Thứ 7" };
        txtSelectedDate.Text = $"{vietnameseDays[(int)date.DayOfWeek]}, {date:dd/MM/yyyy}";

        var events = _monthEvents.ContainsKey(date.Date) ? _monthEvents[date.Date] : new List<CalendarEvent>();
        txtEventSummary.Text = events.Count > 0
            ? $"{events.Count} sự kiện"
            : "Không có sự kiện";

        // Clear old items, keep emptyState
        var toRemove = eventListPanel.Children.Cast<UIElement>()
            .Where(c => c != emptyEventState).ToList();
        foreach (var child in toRemove)
            eventListPanel.Children.Remove(child);

        if (events.Count == 0)
        {
            emptyEventState.Visibility = Visibility.Visible;
            return;
        }

        emptyEventState.Visibility = Visibility.Collapsed;

        // Sort: overdue first, then meetings, then tasks, then documents
        var sorted = events.OrderBy(e => e.Type switch
        {
            EventType.Overdue => 0,
            EventType.DueSoon => 1,
            EventType.Meeting => 2,
            EventType.Task => 3,
            EventType.TaskDone => 4,
            EventType.Document => 5,
            _ => 9
        }).ToList();

        foreach (var evt in sorted)
        {
            var card = new Border
            {
                BorderBrush = new SolidColorBrush(evt.Color),
                BorderThickness = new Thickness(3, 0, 0, 0),
                Background = Brushes.White,
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(12, 8, 12, 8),
                Margin = new Thickness(0, 0, 0, 8),
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Colors.Black,
                    Opacity = 0.08,
                    BlurRadius = 6,
                    ShadowDepth = 1
                }
            };

            var cardStack = new StackPanel();

            // Event type badge
            var typeBadge = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(30, evt.Color.R, evt.Color.G, evt.Color.B)),
                CornerRadius = new CornerRadius(3),
                Padding = new Thickness(6, 2, 6, 2),
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 0, 0, 4)
            };
            typeBadge.Child = new TextBlock
            {
                Text = evt.Type switch
                {
                    EventType.Overdue => "⚠ QUÁ HẠN",
                    EventType.DueSoon => "⏰ SẮP HẠN",
                    EventType.Meeting => "📅 CUỘC HỌP",
                    EventType.Task => "📋 NHIỆM VỤ",
                    EventType.TaskDone => "✅ ĐÃ XONG",
                    EventType.Document => "📄 VĂN BẢN",
                    _ => ""
                },
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(evt.Color)
            };
            cardStack.Children.Add(typeBadge);

            // Title
            cardStack.Children.Add(new TextBlock
            {
                Text = evt.FullLabel,
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(55, 71, 79)),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 2, 0, 4)
            });

            // Detail
            cardStack.Children.Add(new TextBlock
            {
                Text = evt.Detail,
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(117, 117, 117)),
                TextWrapping = TextWrapping.Wrap
            });

            card.Child = cardStack;
            eventListPanel.Children.Add(card);
        }
    }

    #endregion

    #region Helpers

    private static string TruncateText(string text, int maxLen)
    {
        if (string.IsNullOrEmpty(text)) return "";
        return text.Length <= maxLen ? text : text.Substring(0, maxLen) + "…";
    }

    #endregion

    #region Event Models

    private enum EventType
    {
        Overdue,    // 🔴 VB quá hạn
        DueSoon,    // 🟡 VB sắp hạn
        Meeting,    // 🔵 Cuộc họp
        Task,       // 🟢 Nhiệm vụ (chưa xong)
        TaskDone,   // ✅ Nhiệm vụ đã hoàn thành
        Document    // 📄 VB có deadline (chưa quá/sắp hạn)
    }

    private class CalendarEvent
    {
        public EventType Type { get; set; }
        public string ShortLabel { get; set; } = "";  // Hiển thị trong ô lịch (ngắn)
        public string FullLabel { get; set; } = "";    // Tiêu đề đầy đủ (panel bên phải)
        public string Detail { get; set; } = "";       // Chi tiết (panel bên phải)
        public Color Color { get; set; }
    }

    #endregion
}
