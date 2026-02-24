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
        _selectedDate = DateTime.Today; // Auto-select hôm nay
        
        Loaded += (s, e) =>
        {
            RenderCalendar();
            ShowDayDetail(DateTime.Today); // Hiện sự kiện hôm nay ngay khi mở
        };
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
                // Mở dialog ở chế độ "thêm mới" (null = new)
                var dialog = new MeetingEditDialog(null, _meetingService, _documentService)
                {
                    Owner = Window.GetWindow(this)
                };
                
                // Pre-set ngày đã chọn trên lịch thay vì ngày hôm nay
                dialog.Loaded += (s, ev) =>
                {
                    dialog.dpStartDate.SelectedDate = selectedDate;
                    dialog.tpStartTime.SelectedTime = selectedDate.Date.AddHours(8);
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
