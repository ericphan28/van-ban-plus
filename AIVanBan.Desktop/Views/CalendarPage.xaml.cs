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
    private DateTime _currentWeekStart; // Monday of current week
    private DateTime? _selectedDate;
    private CalendarViewMode _viewMode = CalendarViewMode.Month;

    // Event data for current month
    private Dictionary<DateTime, List<CalendarEvent>> _monthEvents = new();

    public CalendarPage(DocumentService documentService)
    {
        InitializeComponent();
        _documentService = documentService;
        _meetingService = new MeetingService();
        _currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        _currentWeekStart = GetMondayOfWeek(DateTime.Today);
        _selectedDate = DateTime.Today; // Auto-select hôm nay
        
        Loaded += (s, e) =>
        {
            RenderCalendar();
            ShowDayDetail(DateTime.Today); // Hiện sự kiện hôm nay ngay khi mở
        };
    }

    #region View Mode Toggle

    private enum CalendarViewMode { Month, Week }

    private void ViewMonth_Click(object sender, RoutedEventArgs e)
    {
        if (_viewMode == CalendarViewMode.Month) return;
        _viewMode = CalendarViewMode.Month;
        UpdateViewToggleButtons();
        RenderCalendar();
    }

    private void ViewWeek_Click(object sender, RoutedEventArgs e)
    {
        if (_viewMode == CalendarViewMode.Week) return;
        _viewMode = CalendarViewMode.Week;
        _currentWeekStart = GetMondayOfWeek(_selectedDate ?? DateTime.Today);
        UpdateViewToggleButtons();
        RenderCalendar();
    }

    private void UpdateViewToggleButtons()
    {
        if (_viewMode == CalendarViewMode.Month)
        {
            btnViewMonth.Background = new SolidColorBrush(Color.FromRgb(21, 101, 192));
            btnViewMonth.Foreground = Brushes.White;
            btnViewWeek.Background = Brushes.Transparent;
            btnViewWeek.Foreground = new SolidColorBrush(Color.FromRgb(21, 101, 192));
            cardMonthView.Visibility = Visibility.Visible;
            cardWeekView.Visibility = Visibility.Collapsed;
        }
        else
        {
            btnViewWeek.Background = new SolidColorBrush(Color.FromRgb(21, 101, 192));
            btnViewWeek.Foreground = Brushes.White;
            btnViewMonth.Background = Brushes.Transparent;
            btnViewMonth.Foreground = new SolidColorBrush(Color.FromRgb(21, 101, 192));
            cardMonthView.Visibility = Visibility.Collapsed;
            cardWeekView.Visibility = Visibility.Visible;
        }
    }

    private static DateTime GetMondayOfWeek(DateTime date)
    {
        int diff = ((int)date.DayOfWeek + 6) % 7; // Mon=0
        return date.Date.AddDays(-diff);
    }

    #endregion

    #region Navigation

    private void PrevMonth_Click(object sender, RoutedEventArgs e)
    {
        if (_viewMode == CalendarViewMode.Month)
        {
            _currentMonth = _currentMonth.AddMonths(-1);
        }
        else
        {
            _currentWeekStart = _currentWeekStart.AddDays(-7);
            _currentMonth = new DateTime(_currentWeekStart.Year, _currentWeekStart.Month, 1);
        }
        RenderCalendar();
    }

    private void NextMonth_Click(object sender, RoutedEventArgs e)
    {
        if (_viewMode == CalendarViewMode.Month)
        {
            _currentMonth = _currentMonth.AddMonths(1);
        }
        else
        {
            _currentWeekStart = _currentWeekStart.AddDays(7);
            _currentMonth = new DateTime(_currentWeekStart.Year, _currentWeekStart.Month, 1);
        }
        RenderCalendar();
    }

    private void Today_Click(object sender, RoutedEventArgs e)
    {
        _currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        _currentWeekStart = GetMondayOfWeek(DateTime.Today);
        _selectedDate = DateTime.Today;
        RenderCalendar();
        ShowDayDetail(DateTime.Today);
    }

    #endregion

    #region Render Calendar

    private void RenderCalendar()
    {
        if (_viewMode == CalendarViewMode.Week)
        {
            RenderWeekView();
            return;
        }
        
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
            BorderBrush = new SolidColorBrush(isSelected 
                ? Color.FromRgb(25, 118, 210) 
                : isToday 
                    ? Color.FromRgb(100, 181, 246) 
                    : Color.FromRgb(230, 230, 230)),
            BorderThickness = new Thickness(isSelected ? 2.5 : isToday ? 1.5 : 0.5),
            Margin = new Thickness(1.5),
            CornerRadius = new CornerRadius(8),
            Background = isToday
                ? new SolidColorBrush(Color.FromRgb(227, 242, 253)) // light blue
                : isSelected
                    ? new SolidColorBrush(Color.FromRgb(232, 245, 253))
                    : isWeekend && isCurrentMonth
                        ? new SolidColorBrush(Color.FromRgb(255, 253, 248)) // warm tint for weekends
                        : Brushes.White,
            Cursor = Cursors.Hand,
            Tag = date,
            MinHeight = 80
        };

        border.MouseLeftButtonDown += DayCell_Click;

        // Hover effect
        border.MouseEnter += (s, e) =>
        {
            if (!isSelected && !isToday)
                border.Background = new SolidColorBrush(Color.FromRgb(245, 248, 255));
        };
        border.MouseLeave += (s, e) =>
        {
            if (!isSelected && !isToday)
                border.Background = isWeekend && isCurrentMonth
                    ? new SolidColorBrush(Color.FromRgb(255, 253, 248))
                    : Brushes.White;
        };

        var stack = new StackPanel { Margin = new Thickness(6, 4, 6, 4) };

        // Day number — bigger + bolder
        var dayText = new TextBlock
        {
            Text = date.Day.ToString(),
            FontSize = 15,
            FontWeight = isToday ? FontWeights.ExtraBold : FontWeights.SemiBold,
            Foreground = !isCurrentMonth
                ? new SolidColorBrush(Color.FromRgb(200, 200, 200))
                : isToday
                    ? new SolidColorBrush(Color.FromRgb(21, 101, 192))
                    : isWeekend
                        ? (date.DayOfWeek == DayOfWeek.Sunday
                            ? new SolidColorBrush(Color.FromRgb(198, 40, 40))
                            : new SolidColorBrush(Color.FromRgb(230, 81, 0)))
                        : new SolidColorBrush(Color.FromRgb(55, 71, 79)),
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 0, 2, 2)
        };

        // Today badge — circle behind number
        if (isToday)
        {
            var todayBadge = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(21, 101, 192)),
                CornerRadius = new CornerRadius(14),
                Width = 28, Height = 28,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 0, 0, 2)
            };
            dayText.Foreground = Brushes.White;
            dayText.HorizontalAlignment = HorizontalAlignment.Center;
            dayText.VerticalAlignment = VerticalAlignment.Center;
            dayText.Margin = new Thickness(0);
            todayBadge.Child = dayText;
            stack.Children.Add(todayBadge);
        }
        else
        {
            stack.Children.Add(dayText);
        }

        // Event indicators (max 3 visible, then "+N") — bigger and more readable
        int shown = 0;
        foreach (var evt in events.Take(3))
        {
            var indicator = new Border
            {
                Background = new SolidColorBrush(evt.Color),
                CornerRadius = new CornerRadius(3),
                Padding = new Thickness(4, 2, 4, 2),
                Margin = new Thickness(0, 2, 0, 0)
            };
            var label = new TextBlock
            {
                Text = evt.ShortLabel,
                FontSize = 11,
                FontWeight = FontWeights.Medium,
                Foreground = Brushes.White,
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            indicator.Child = label;
            stack.Children.Add(indicator);
            shown++;
        }

        if (events.Count > 3)
        {
            var moreText = new TextBlock
            {
                Text = $"+{events.Count - 3} sự kiện khác",
                FontSize = 11,
                FontWeight = FontWeights.Medium,
                Foreground = new SolidColorBrush(Color.FromRgb(100, 100, 100)),
                Margin = new Thickness(2, 2, 0, 0)
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

    #region Week View

    /// <summary>
    /// Render chế độ xem Tuần — hiển thị 7 ngày (T2→CN) với time-slot 7:00–18:00
    /// </summary>
    private void RenderWeekView()
    {
        var weekEnd = _currentWeekStart.AddDays(6);
        txtMonthYear.Text = $"{_currentWeekStart:dd/MM} — {weekEnd:dd/MM/yyyy}";
        
        // Load events for the week
        LoadMonthEvents(); // Reuses same loader (range extends ±7 days)

        // Update day headers
        var dayNames = new[] { "T2", "T3", "T4", "T5", "T6", "T7", "CN" };
        var dayHeaders = new[] { txtWeekDay0, txtWeekDay1, txtWeekDay2, txtWeekDay3, txtWeekDay4, txtWeekDay5, txtWeekDay6 };
        for (int i = 0; i < 7; i++)
        {
            var date = _currentWeekStart.AddDays(i);
            var isToday = date.Date == DateTime.Today;
            dayHeaders[i].Text = $"{dayNames[i]} {date:dd/MM}";
            dayHeaders[i].FontWeight = isToday ? FontWeights.ExtraBold : FontWeights.Bold;
            dayHeaders[i].Foreground = isToday 
                ? new SolidColorBrush(Color.FromRgb(21, 101, 192))
                : i == 5 ? new SolidColorBrush(Color.FromRgb(230, 81, 0))
                : i == 6 ? new SolidColorBrush(Color.FromRgb(198, 40, 40))
                : new SolidColorBrush(Color.FromRgb(55, 71, 79));
        }

        // Build time-slot grid
        weekTimeGrid.Children.Clear();
        weekTimeGrid.RowDefinitions.Clear();
        weekTimeGrid.ColumnDefinitions.Clear();

        // 8 columns: 1 for time labels + 7 for days
        weekTimeGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
        for (int i = 0; i < 7; i++)
            weekTimeGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        // 12 rows: 7:00 → 18:00
        int startHour = 7, endHour = 18;
        for (int h = startHour; h <= endHour; h++)
        {
            weekTimeGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(60) });
        }

        // Time labels + grid lines
        for (int h = startHour; h <= endHour; h++)
        {
            int row = h - startHour;
            
            // Time label
            var timeLabel = new TextBlock
            {
                Text = $"{h:D2}:00",
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(130, 130, 130)),
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, -6, 0, 0)
            };
            Grid.SetRow(timeLabel, row);
            Grid.SetColumn(timeLabel, 0);
            weekTimeGrid.Children.Add(timeLabel);

            // Horizontal grid lines for each day column
            for (int d = 0; d < 7; d++)
            {
                var line = new Border
                {
                    BorderBrush = new SolidColorBrush(Color.FromRgb(238, 238, 238)),
                    BorderThickness = new Thickness(0.5, 0.5, 0.5, 0),
                    Background = _currentWeekStart.AddDays(d).Date == DateTime.Today
                        ? new SolidColorBrush(Color.FromArgb(15, 21, 101, 192))
                        : Brushes.Transparent,
                    Tag = _currentWeekStart.AddDays(d),
                    Cursor = Cursors.Hand
                };
                line.MouseLeftButtonDown += WeekDaySlot_Click;
                Grid.SetRow(line, row);
                Grid.SetColumn(line, d + 1);
                weekTimeGrid.Children.Add(line);
            }
        }

        // Place events on the time grid
        for (int d = 0; d < 7; d++)
        {
            var date = _currentWeekStart.AddDays(d);
            if (!_monthEvents.ContainsKey(date.Date)) continue;

            var events = _monthEvents[date.Date];
            var meetingEvents = events.Where(e => e.Type == EventType.Meeting).ToList();
            var otherEvents = events.Where(e => e.Type != EventType.Meeting).ToList();

            // Place meeting events at their time slots
            foreach (var evt in meetingEvents)
            {
                if (evt.MeetingId == null) continue;
                try
                {
                    var meeting = _meetingService.GetMeetingById(evt.MeetingId);
                    if (meeting == null) continue;

                    int startRow = Math.Max(0, meeting.StartTime.Hour - startHour);
                    int endRow = meeting.EndTime.HasValue 
                        ? Math.Min(endHour - startHour, meeting.EndTime.Value.Hour - startHour)
                        : startRow + 1;
                    int span = Math.Max(1, endRow - startRow);

                    var eventBlock = CreateWeekEventBlock(evt, meeting.StartTime.ToString("HH:mm") + 
                        (meeting.EndTime.HasValue ? $"-{meeting.EndTime.Value:HH:mm}" : ""));
                    eventBlock.Tag = evt.MeetingId;
                    eventBlock.MouseLeftButtonDown += EventCard_OpenMeeting;
                    
                    Grid.SetRow(eventBlock, startRow);
                    Grid.SetRowSpan(eventBlock, span);
                    Grid.SetColumn(eventBlock, d + 1);
                    weekTimeGrid.Children.Add(eventBlock);
                }
                catch { /* Skip invalid meetings */ }
            }

            // Place other events (docs, tasks) at the top (row 0)
            if (otherEvents.Count > 0)
            {
                var summaryBlock = new Border
                {
                    Background = new SolidColorBrush(Color.FromArgb(200, 255, 243, 224)),
                    CornerRadius = new CornerRadius(4),
                    Padding = new Thickness(4, 2, 4, 2),
                    Margin = new Thickness(2, 2, 2, 0),
                    VerticalAlignment = VerticalAlignment.Top,
                    Cursor = Cursors.Hand,
                    Tag = date
                };
                summaryBlock.MouseLeftButtonDown += WeekDaySlot_Click;
                summaryBlock.Child = new TextBlock
                {
                    Text = $"📋 {otherEvents.Count} sự kiện",
                    FontSize = 10,
                    Foreground = new SolidColorBrush(Color.FromRgb(230, 81, 0)),
                    TextTrimming = TextTrimming.CharacterEllipsis
                };
                Grid.SetRow(summaryBlock, 0);
                Grid.SetColumn(summaryBlock, d + 1);
                weekTimeGrid.Children.Add(summaryBlock);
            }
        }
    }

    private Border CreateWeekEventBlock(CalendarEvent evt, string timeText)
    {
        var block = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(220, evt.Color.R, evt.Color.G, evt.Color.B)),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(6, 4, 6, 4),
            Margin = new Thickness(2),
            Cursor = Cursors.Hand,
            ToolTip = $"{evt.FullLabel}\n{evt.Detail}"
        };

        var stack = new StackPanel();
        stack.Children.Add(new TextBlock
        {
            Text = timeText,
            FontSize = 10,
            FontWeight = FontWeights.Bold,
            Foreground = Brushes.White
        });
        stack.Children.Add(new TextBlock
        {
            Text = evt.FullLabel,
            FontSize = 11,
            Foreground = Brushes.White,
            TextTrimming = TextTrimming.CharacterEllipsis,
            TextWrapping = TextWrapping.Wrap,
            MaxHeight = 36
        });

        block.Child = stack;
        return block;
    }

    private void WeekDaySlot_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is Border border && border.Tag is DateTime date)
        {
            _selectedDate = date;
            RenderCalendar();
            ShowDayDetail(date);
        }
    }

    #endregion

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
                ShortLabel = $"📄 {TruncateText(doc.Number, 18)}",
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
                    ShortLabel = $"🔵 {TruncateText(meeting.Title, 18)}",
                    FullLabel = meeting.Title,
                    Detail = $"Thời gian: {meeting.StartTime:HH:mm} - {meeting.EndTime:HH:mm}\n" +
                             $"Địa điểm: {meeting.Location}\n" +
                             $"Chủ trì: {meeting.ChairPerson}\n" +
                             $"Trạng thái: {MeetingHelper.GetStatusName(meeting.Status)}",
                    Color = Color.FromRgb(21, 101, 192), // Blue
                    MeetingId = meeting.Id
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
                            ShortLabel = isTaskDone ? $"✅ {TruncateText(task.Title, 16)}" : $"📋 {TruncateText(task.Title, 16)}",
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
        var vietnameseDays = new[] { "Chủ nhật", "Thứ Hai", "Thứ Ba", "Thứ Tư", "Thứ Năm", "Thứ Sáu", "Thứ Bảy" };
        txtSelectedDate.Text = $"{vietnameseDays[(int)date.DayOfWeek]}, {date:dd/MM/yyyy}";

        var events = _monthEvents.ContainsKey(date.Date) ? _monthEvents[date.Date] : new List<CalendarEvent>();
        txtEventSummary.Text = events.Count > 0
            ? $"📌 {events.Count} sự kiện"
            : "Không có sự kiện nào";

        // Clear old items, keep emptyState
        var toRemove = eventListPanel.Children.Cast<UIElement>()
            .Where(c => c != emptyEventState).ToList();
        foreach (var child in toRemove)
            eventListPanel.Children.Remove(child);

        // === NÚT THÊM CUỘC HỌP ===
        var addMeetingBtn = new Button
        {
            Content = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Children =
                {
                    new MaterialDesignThemes.Wpf.PackIcon
                    {
                        Kind = MaterialDesignThemes.Wpf.PackIconKind.Plus,
                        Width = 18, Height = 18,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(0, 0, 8, 0)
                    },
                    new TextBlock
                    {
                        Text = "Thêm cuộc họp",
                        VerticalAlignment = VerticalAlignment.Center,
                        FontSize = 14,
                        FontWeight = FontWeights.Medium
                    }
                }
            },
            Tag = date,
            Padding = new Thickness(16, 10, 16, 10),
            Margin = new Thickness(0, 0, 0, 14),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Cursor = Cursors.Hand,
            Background = new SolidColorBrush(Color.FromRgb(21, 101, 192)),
            Foreground = Brushes.White,
            BorderThickness = new Thickness(0),
            FontSize = 14
        };
        // Round corners via style
        addMeetingBtn.Resources.Add(typeof(Border), new Style(typeof(Border))
        {
            Setters = { new Setter(Border.CornerRadiusProperty, new CornerRadius(8)) }
        });
        addMeetingBtn.Click += AddMeetingFromCalendar_Click;
        eventListPanel.Children.Add(addMeetingBtn);

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
                BorderThickness = new Thickness(4, 0, 0, 0),
                Background = Brushes.White,
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(16, 12, 16, 12),
                Margin = new Thickness(0, 0, 0, 10),
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Colors.Black,
                    Opacity = 0.1,
                    BlurRadius = 10,
                    ShadowDepth = 2
                }
            };

            var cardStack = new StackPanel();

            // Event type badge — bigger, more visible
            var typeBadge = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(35, evt.Color.R, evt.Color.G, evt.Color.B)),
                CornerRadius = new CornerRadius(5),
                Padding = new Thickness(10, 4, 10, 4),
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 0, 0, 6)
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
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(evt.Color)
            };
            cardStack.Children.Add(typeBadge);

            // Title — bigger, more prominent
            cardStack.Children.Add(new TextBlock
            {
                Text = evt.FullLabel,
                FontSize = 15,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(33, 33, 33)),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 2, 0, 6)
            });

            // Detail — bigger, better line spacing
            cardStack.Children.Add(new TextBlock
            {
                Text = evt.Detail,
                FontSize = 13,
                Foreground = new SolidColorBrush(Color.FromRgb(100, 100, 100)),
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 20
            });

            card.Child = cardStack;

            // Nếu là cuộc họp → click để mở sửa
            if (evt.Type == EventType.Meeting && !string.IsNullOrEmpty(evt.MeetingId))
            {
                card.Cursor = Cursors.Hand;
                card.Tag = evt.MeetingId;
                card.MouseLeftButtonDown += EventCard_OpenMeeting;
                card.ToolTip = "Click để mở cuộc họp";
            }

            eventListPanel.Children.Add(card);
        }
        
        // === SẮP TỚI TRONG TUẦN ===
        ShowUpcomingThisWeek(date);
    }
    
    /// <summary>
    /// Hiển thị danh sách cuộc họp sắp tới trong tuần (dưới phần chi tiết ngày đã chọn).
    /// </summary>
    private void ShowUpcomingThisWeek(DateTime selectedDate)
    {
        try
        {
            var today = DateTime.Today;
            var weekEnd = today.AddDays(7);
            var upcoming = _meetingService.GetMeetingsByDateRange(today, weekEnd)
                .Where(m => m.StartTime.Date != selectedDate.Date) // Bỏ ngày đang xem
                .Where(m => m.StartTime >= DateTime.Now) // Chỉ lấy cuộc họp chưa diễn ra
                .OrderBy(m => m.StartTime)
                .Take(5)
                .ToList();
            
            if (upcoming.Count == 0) return;
            
            // Separator
            var separator = new Border
            {
                BorderBrush = new SolidColorBrush(Color.FromRgb(224, 224, 224)),
                BorderThickness = new Thickness(0, 1, 0, 0),
                Margin = new Thickness(0, 18, 0, 14)
            };
            eventListPanel.Children.Add(separator);
            
            // Title
            var sectionTitle = new TextBlock
            {
                Text = "📆 SẮP TỚI TRONG TUẦN",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(100, 100, 100)),
                Margin = new Thickness(0, 0, 0, 10)
            };
            eventListPanel.Children.Add(sectionTitle);
            
            foreach (var meeting in upcoming)
            {
                var meetingCard = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(245, 249, 255)),
                    CornerRadius = new CornerRadius(8),
                    Padding = new Thickness(14, 10, 14, 10),
                    Margin = new Thickness(0, 0, 0, 6),
                    Cursor = Cursors.Hand,
                    Tag = meeting.Id
                };
                meetingCard.MouseLeftButtonDown += EventCard_OpenMeeting;
                meetingCard.ToolTip = "Click để mở cuộc họp";
                
                var meetingStack = new StackPanel();
                
                // Time + Title
                var relTime = GetRelativeTimeText(meeting.StartTime);
                meetingStack.Children.Add(new TextBlock
                {
                    Text = $"{meeting.StartTime:dd/MM HH:mm} — {meeting.Title}",
                    FontSize = 14,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush(Color.FromRgb(33, 33, 33)),
                    TextTrimming = TextTrimming.CharacterEllipsis
                });
                
                // Relative time + location
                var subText = relTime;
                if (!string.IsNullOrEmpty(meeting.Location))
                    subText += $" • {meeting.Location}";
                meetingStack.Children.Add(new TextBlock
                {
                    Text = subText,
                    FontSize = 12,
                    Foreground = new SolidColorBrush(Color.FromRgb(130, 130, 130)),
                    Margin = new Thickness(0, 3, 0, 0)
                });
                
                meetingCard.Child = meetingStack;
                eventListPanel.Children.Add(meetingCard);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Calendar: Error loading upcoming: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Tính thời gian tương đối (VD: "Sau 2 giờ", "Ngày mai", "Còn 3 ngày")
    /// </summary>
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

    #endregion

    #region Meeting Interactions

    /// <summary>
    /// Click nút "Thêm cuộc họp" trong panel chi tiết ngày → mở dialog tạo mới, pre-set ngày đã chọn.
    /// </summary>
    private void AddMeetingFromCalendar_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is DateTime selectedDate)
        {
            try
            {
                // Mở dialog chế độ TẠO NHANH với ngày đã chọn
                var dialog = new MeetingEditDialog(_meetingService, _documentService, selectedDate)
                {
                    Owner = Window.GetWindow(this)
                };

                if (dialog.ShowDialog() == true)
                {
                    // Refresh lịch sau khi thêm
                    RenderCalendar();
                    ShowDayDetail(selectedDate);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tạo cuộc họp:\n{ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    /// <summary>
    /// Click vào event card cuộc họp trong panel chi tiết → mở dialog sửa cuộc họp.
    /// </summary>
    private void EventCard_OpenMeeting(object sender, MouseButtonEventArgs e)
    {
        if (sender is Border card && card.Tag is string meetingId)
        {
            try
            {
                var meeting = _meetingService.GetMeetingById(meetingId);
                if (meeting == null)
                {
                    MessageBox.Show("Không tìm thấy cuộc họp!", "Lỗi",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var dialog = new MeetingEditDialog(meeting, _meetingService, _documentService)
                {
                    Owner = Window.GetWindow(this)
                };

                if (dialog.ShowDialog() == true)
                {
                    // Refresh lịch sau khi sửa
                    RenderCalendar();
                    if (_selectedDate.HasValue)
                        ShowDayDetail(_selectedDate.Value);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi mở cuộc họp:\n{ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
        public string? MeetingId { get; set; }         // ID cuộc họp (để mở sửa khi click)
    }

    #endregion
}
