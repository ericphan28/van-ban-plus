using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using AIVanBan.Core.Models;
using AIVanBan.Core.Services;

namespace AIVanBan.Desktop.Views;

public partial class DashboardPage : Page
{
    private readonly DocumentService _documentService;
    private readonly MeetingService _meetingService;

    // View models for ItemsControl binding
    public class AlertItem
    {
        public string Icon { get; set; } = "";
        public string Message { get; set; } = "";
        public string Badge { get; set; } = "";
        public Brush BadgeColor { get; set; } = Brushes.Gray;
        public Brush Background { get; set; } = Brushes.Transparent;
    }

    public class ChartItem
    {
        public string TypeName { get; set; } = "";
        public int Count { get; set; }
        public double BarWidth { get; set; }
        public Brush BarColor { get; set; } = Brushes.CornflowerBlue;
    }

    public class ActivityItem
    {
        public string Title { get; set; } = "";
        public string TimeAgo { get; set; } = "";
        public string IconKind { get; set; } = "FileDocument";
        public Brush IconBg { get; set; } = Brushes.LightBlue;
        public Brush IconFg { get; set; } = Brushes.DarkBlue;
    }

    public class TaskItem
    {
        public string Title { get; set; } = "";
        public string DeadlineText { get; set; } = "";
        public Brush DeadlineColor { get; set; } = Brushes.Gray;
        public string StatusIcon { get; set; } = "CheckboxBlankCircleOutline";
        public Brush StatusColor { get; set; } = Brushes.Gray;
        public Brush Background { get; set; } = Brushes.Transparent;
        public string MeetingLabel { get; set; } = "";
        public TextDecorationCollection? Strikethrough { get; set; }
    }

    public class TemplateItem
    {
        public string Name { get; set; } = "";
        public string UsageText { get; set; } = "0 lần";
        public string TemplateId { get; set; } = "";
    }

    public DashboardPage(DocumentService documentService)
    {
        InitializeComponent();
        _documentService = documentService;
        _meetingService = new MeetingService();

        Loaded += DashboardPage_Loaded;
    }

    private void DashboardPage_Loaded(object sender, RoutedEventArgs e)
    {
        LoadAllDashboardData();
    }

    private void RefreshDashboard_Click(object sender, RoutedEventArgs e)
    {
        LoadAllDashboardData();
    }

    private async void LoadAllDashboardData()
    {
        try
        {
            LoadGreeting();
            
            // Load stat data on background thread to avoid UI freeze
            var allDocs = await Task.Run(() => _documentService.GetAllDocuments());
            var allTemplates = await Task.Run(() => _documentService.GetAllTemplates());
            
            LoadStatCards(allDocs);
            LoadAlerts(allDocs);
            LoadUpcomingSchedule();
            LoadDocumentsByType(allDocs);
            LoadRecentActivity(allDocs);
            LoadTasks();
            LoadTemplates(allTemplates);
            LoadTrendChart(allDocs);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Dashboard load error: {ex.Message}");
        }
    }

    // ═══════════════════════════════════════
    // GREETING
    // ═══════════════════════════════════════
    private void LoadGreeting()
    {
        var hour = DateTime.Now.Hour;
        var greeting = hour switch
        {
            < 12 => "Chào buổi sáng",
            < 18 => "Chào buổi chiều",
            _ => "Chào buổi tối"
        };

        var settings = AppSettingsService.Load();
        var name = !string.IsNullOrWhiteSpace(settings.UserFullName) ? settings.UserFullName : Environment.UserName;
        txtGreeting.Text = $"{greeting}, {name}!";

        var culture = new CultureInfo("vi-VN");
        txtDate.Text = DateTime.Now.ToString("dddd, 'ngày' dd 'tháng' MM 'năm' yyyy", culture);
    }

    // ═══════════════════════════════════════
    // STAT CARDS
    // ═══════════════════════════════════════
    private void LoadStatCards(List<Document> allDocs)
    {
        var now = DateTime.Now;
        var monthStart = new DateTime(now.Year, now.Month, 1);
        var weekStart = now.Date.AddDays(-(int)now.DayOfWeek + (int)DayOfWeek.Monday);
        if (weekStart > now.Date) weekStart = weekStart.AddDays(-7);

        var monthDocs = allDocs.Where(d => d.IssueDate >= monthStart && d.IssueDate <= now).ToList();
        var weekDocs = allDocs.Where(d => d.CreatedDate >= weekStart && d.CreatedDate <= now).ToList();

        // Total
        txtStatTotal.Text = allDocs.Count.ToString("N0");
        var weekTotal = weekDocs.Count;
        txtStatTotalDelta.Text = $"+{weekTotal} tuần này";

        // VB Đến tháng
        var denMonth = monthDocs.Count(d => d.Direction == Direction.Den);
        var denWeek = weekDocs.Count(d => d.Direction == Direction.Den);
        txtStatDen.Text = denMonth.ToString();
        txtStatDenDelta.Text = $"+{denWeek} tuần này";

        // VB Đi tháng
        var diMonth = monthDocs.Count(d => d.Direction == Direction.Di);
        var diWeek = weekDocs.Count(d => d.Direction == Direction.Di);
        txtStatDi.Text = diMonth.ToString();
        txtStatDiDelta.Text = $"+{diWeek} tuần này";

        // Nội bộ tháng
        var noiBo = monthDocs.Count(d => d.Direction == Direction.NoiBo);
        var noiBWeek = weekDocs.Count(d => d.Direction == Direction.NoiBo);
        txtStatNoiBo.Text = noiBo.ToString();
        txtStatNoiBoDelta.Text = $"+{noiBWeek} tuần này";

        // Họp tháng này
        var meetingsMonth = _meetingService.GetMeetingsCountThisMonth();
        var meetingsWeek = _meetingService.GetMeetingsByDateRange(weekStart, now).Count;
        txtStatMeeting.Text = meetingsMonth.ToString();
        txtStatMeetingDelta.Text = $"+{meetingsWeek} tuần này";
    }

    // ═══════════════════════════════════════
    // ALERTS
    // ═══════════════════════════════════════
    private void LoadAlerts(List<Document> allDocs)
    {
        var alerts = new List<AlertItem>();
        var now = DateTime.Now;

        // 0a. VB đến QUÁ HẠN xử lý — Theo Điều 24, NĐ 30/2020
        var overdueDocuments = _documentService.GetOverdueDocuments();
        if (overdueDocuments.Count > 0)
        {
            alerts.Add(new AlertItem
            {
                Icon = "🔴",
                Message = $"{overdueDocuments.Count} VB đến đã quá hạn xử lý!",
                Badge = $"{overdueDocuments.Count}",
                BadgeColor = new SolidColorBrush(Color.FromRgb(198, 40, 40)),
                Background = new SolidColorBrush(Color.FromRgb(255, 235, 238)) // #FFEBEE
            });
        }
        
        // 0b. VB đến SẮP HẾT HẠN (3 ngày tới)
        var dueSoonDocuments = _documentService.GetDocumentsDueSoon(3);
        if (dueSoonDocuments.Count > 0)
        {
            alerts.Add(new AlertItem
            {
                Icon = "🟡",
                Message = $"{dueSoonDocuments.Count} VB đến sắp hết hạn xử lý (3 ngày)",
                Badge = $"{dueSoonDocuments.Count}",
                BadgeColor = new SolidColorBrush(Color.FromRgb(230, 81, 0)),
                Background = new SolidColorBrush(Color.FromRgb(255, 243, 224)) // #FFF3E0
            });
        }

        // 1. VB chờ duyệt lâu (> 3 ngày)
        var pendingApproval = allDocs.Where(d =>
            d.WorkflowStatus == DocumentStatus.PendingApproval &&
            d.CreatedDate < now.AddDays(-3)).ToList();
        if (pendingApproval.Count > 0)
        {
            alerts.Add(new AlertItem
            {
                Icon = "🔴",
                Message = $"{pendingApproval.Count} VB chờ duyệt quá 3 ngày",
                Badge = $"{pendingApproval.Count}",
                BadgeColor = new SolidColorBrush(Color.FromRgb(198, 40, 40)),
                Background = new SolidColorBrush(Color.FromRgb(255, 235, 238)) // #FFEBEE
            });
        }

        // 2. Nhiệm vụ quá hạn
        var overdueTasks = _meetingService.GetOverdueTaskCount();
        if (overdueTasks > 0)
        {
            alerts.Add(new AlertItem
            {
                Icon = "🔴",
                Message = $"{overdueTasks} nhiệm vụ đã quá hạn",
                Badge = $"{overdueTasks}",
                BadgeColor = new SolidColorBrush(Color.FromRgb(198, 40, 40)),
                Background = new SolidColorBrush(Color.FromRgb(255, 235, 238))
            });
        }

        // 3. VB đến chưa xử lý trong tuần
        var weekStart = now.Date.AddDays(-(int)now.DayOfWeek + (int)DayOfWeek.Monday);
        if (weekStart > now.Date) weekStart = weekStart.AddDays(-7);
        var unprocessedIncoming = allDocs.Where(d =>
            d.Direction == Direction.Den &&
            d.WorkflowStatus == DocumentStatus.Draft &&
            d.CreatedDate >= weekStart).ToList();
        if (unprocessedIncoming.Count > 0)
        {
            alerts.Add(new AlertItem
            {
                Icon = "🟡",
                Message = $"{unprocessedIncoming.Count} VB đến chưa xử lý (tuần này)",
                Badge = $"{unprocessedIncoming.Count}",
                BadgeColor = new SolidColorBrush(Color.FromRgb(230, 81, 0)),
                Background = new SolidColorBrush(Color.FromRgb(255, 243, 224)) // #FFF3E0
            });
        }

        // 4. Cuộc họp hôm nay
        var todayMeetings = _meetingService.GetTodayMeetings();
        if (todayMeetings.Count > 0)
        {
            alerts.Add(new AlertItem
            {
                Icon = "🟡",
                Message = $"{todayMeetings.Count} cuộc họp hôm nay",
                Badge = $"{todayMeetings.Count}",
                BadgeColor = new SolidColorBrush(Color.FromRgb(230, 81, 0)),
                Background = new SolidColorBrush(Color.FromRgb(255, 243, 224))
            });
        }

        // 5. Nhiệm vụ sắp hết hạn (7 ngày tới)
        var pendingTasks = _meetingService.GetPendingTaskCount();
        var upcomingDeadlineTasks = GetTasksWithUpcomingDeadline(7);
        if (upcomingDeadlineTasks > 0)
        {
            alerts.Add(new AlertItem
            {
                Icon = "🟢",
                Message = $"{upcomingDeadlineTasks} nhiệm vụ hết hạn trong 7 ngày tới",
                Badge = $"{upcomingDeadlineTasks}",
                BadgeColor = new SolidColorBrush(Color.FromRgb(46, 125, 50)),
                Background = new SolidColorBrush(Color.FromRgb(232, 245, 233)) // #E8F5E9
            });
        }

        if (alerts.Count == 0)
        {
            pnlNoAlerts.Visibility = Visibility.Visible;
            alertList.Visibility = Visibility.Collapsed;
        }
        else
        {
            pnlNoAlerts.Visibility = Visibility.Collapsed;
            alertList.Visibility = Visibility.Visible;
            alertList.ItemsSource = alerts;
        }
    }

    private int GetTasksWithUpcomingDeadline(int days)
    {
        var now = DateTime.Now;
        var deadline = now.AddDays(days);
        var allMeetings = _meetingService.GetAllMeetings();
        return allMeetings
            .SelectMany(m => m.Tasks ?? new List<MeetingTask>())
            .Count(t => (t.TaskStatus == MeetingTaskStatus.NotStarted ||
                         t.TaskStatus == MeetingTaskStatus.InProgress) &&
                        t.Deadline.HasValue && t.Deadline.Value >= now && t.Deadline.Value <= deadline);
    }

    // ═══════════════════════════════════════
    // TODAY SCHEDULE
    // ═══════════════════════════════════════
    private void LoadUpcomingSchedule()
    {
        // Lấy cuộc họp 7 ngày tới (bao gồm hôm nay)
        var todayStart = DateTime.Today;
        var meetings = _meetingService.GetUpcomingMeetings(7);
        
        // Thêm cuộc họp hôm nay đã qua (để cán bộ biết cuộc nào đã họp rồi)
        var todayPast = _meetingService.GetTodayMeetings()
            .Where(m => m.Status == MeetingStatus.Completed || m.Status == MeetingStatus.InProgress)
            .ToList();
        
        var allMeetings = todayPast
            .Union(meetings, new MeetingIdComparer())
            .OrderBy(m => m.StartTime)
            .ToList();

        if (allMeetings.Count == 0)
        {
            pnlNoSchedule.Visibility = Visibility.Visible;
            scheduleList.Visibility = Visibility.Collapsed;
            txtScheduleCount.Text = "";
            return;
        }

        pnlNoSchedule.Visibility = Visibility.Collapsed;
        scheduleList.Visibility = Visibility.Visible;
        txtScheduleCount.Text = $"({allMeetings.Count})";

        // Group meetings by date
        var grouped = allMeetings.GroupBy(m => m.StartTime.Date).OrderBy(g => g.Key);
        var items = new List<object>();
        var viCulture = new CultureInfo("vi-VN");

        foreach (var group in grouped)
        {
            var dayLabel = GetDayLabel(group.Key, viCulture);
            items.Add(new ScheduleDayHeader
            {
                DayLabel = dayLabel,
                DateText = group.Key.ToString("dd/MM/yyyy"),
                MeetingCount = group.Count()
            });

            foreach (var m in group.OrderBy(x => x.StartTime))
            {
                var item = new ScheduleMeetingItem
                {
                    Time = m.IsAllDay ? "Cả ngày" : m.StartTime.ToString("HH:mm"),
                    Title = m.Title,
                    Location = string.IsNullOrEmpty(m.Location) ? "" : m.Location,
                    ChairPerson = string.IsNullOrEmpty(m.ChairPerson) ? "" : m.ChairPerson,
                    FormatText = GetFormatText(m.Format),
                    FormatBg = GetFormatBrush(m.Format),
                    FormatFg = Brushes.White,
                    PriorityBrush = GetPriorityBrush(m.Priority),
                    TaskCount = m.Tasks?.Count ?? 0,
                    AttendeeCount = m.Attendees?.Count ?? 0,
                    StatusText = GetStatusText(m.Status),
                    StatusBrush = GetStatusBrush(m.Status),
                    LocationVisible = string.IsNullOrEmpty(m.Location) ? Visibility.Collapsed : Visibility.Visible,
                    ChairVisible = string.IsNullOrEmpty(m.ChairPerson) ? Visibility.Collapsed : Visibility.Visible,
                    TaskVisible = (m.Tasks?.Count ?? 0) > 0 ? Visibility.Visible : Visibility.Collapsed
                };
                items.Add(item);
            }
        }

        scheduleList.ItemsSource = items;
    }

    private class MeetingIdComparer : IEqualityComparer<Meeting>
    {
        public bool Equals(Meeting? x, Meeting? y) => x?.Id == y?.Id;
        public int GetHashCode(Meeting obj) => obj.Id?.GetHashCode() ?? 0;
    }

    private string GetDayLabel(DateTime date, CultureInfo culture)
    {
        var today = DateTime.Today;
        if (date.Date == today) return "HÔM NAY";
        if (date.Date == today.AddDays(1)) return "NGÀY MAI";
        if (date.Date == today.AddDays(2)) return "NGÀY KIA";
        
        var dayOfWeek = date.ToString("dddd", culture);
        dayOfWeek = char.ToUpper(dayOfWeek[0]) + dayOfWeek.Substring(1);
        return $"{dayOfWeek}, {date:dd/MM}";
    }

    private string GetFormatText(MeetingFormat format) => format switch
    {
        MeetingFormat.TrucTiep => "Trực tiếp",
        MeetingFormat.TrucTuyen => "Trực tuyến",
        MeetingFormat.KetHop => "Kết hợp",
        _ => ""
    };

    private Brush GetFormatBrush(MeetingFormat format) => format switch
    {
        MeetingFormat.TrucTiep => new SolidColorBrush(Color.FromRgb(0x43, 0xA0, 0x47)),  // Green
        MeetingFormat.TrucTuyen => new SolidColorBrush(Color.FromRgb(0x15, 0x65, 0xC0)),  // Blue
        MeetingFormat.KetHop => new SolidColorBrush(Color.FromRgb(0x7B, 0x1F, 0xA2)),     // Purple
        _ => Brushes.Gray
    };

    private Brush GetPriorityBrush(int priority) => priority switch
    {
        5 => new SolidColorBrush(Color.FromRgb(0xD3, 0x2F, 0x2F)),  // Đỏ đậm - Rất cao
        4 => new SolidColorBrush(Color.FromRgb(0xE6, 0x4A, 0x19)),  // Cam đỏ - Cao
        3 => new SolidColorBrush(Color.FromRgb(0xFF, 0x8F, 0x00)),  // Cam - Trung bình
        2 => new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50)),  // Xanh lá - Thấp
        _ => new SolidColorBrush(Color.FromRgb(0x9E, 0x9E, 0x9E))   // Xám - Rất thấp
    };

    private string GetStatusText(MeetingStatus status) => status switch
    {
        MeetingStatus.Scheduled => "Sắp họp",
        MeetingStatus.InProgress => "Đang họp",
        MeetingStatus.Completed => "Đã xong",
        MeetingStatus.Postponed => "Hoãn",
        MeetingStatus.Cancelled => "Hủy",
        _ => ""
    };

    private Brush GetStatusBrush(MeetingStatus status) => status switch
    {
        MeetingStatus.Scheduled => new SolidColorBrush(Color.FromRgb(0x15, 0x65, 0xC0)),
        MeetingStatus.InProgress => new SolidColorBrush(Color.FromRgb(0x2E, 0x7D, 0x32)),
        MeetingStatus.Completed => new SolidColorBrush(Color.FromRgb(0x75, 0x75, 0x75)),
        MeetingStatus.Postponed => new SolidColorBrush(Color.FromRgb(0xEF, 0x6C, 0x00)),
        MeetingStatus.Cancelled => new SolidColorBrush(Color.FromRgb(0xC6, 0x28, 0x28)),
        _ => Brushes.Gray
    };

    // ═══════════════════════════════════════
    // DOCUMENTS BY TYPE (Bar chart)
    // ═══════════════════════════════════════
    private void LoadDocumentsByType(List<Document> allDocs)
    {
        var now = DateTime.Now;
        var monthStart = new DateTime(now.Year, now.Month, 1);
        var monthDocs = allDocs.Where(d => d.IssueDate >= monthStart && d.IssueDate <= now).ToList();

        if (monthDocs.Count == 0)
        {
            pnlNoDocTypes.Visibility = Visibility.Visible;
            chartByType.Visibility = Visibility.Collapsed;
            return;
        }

        pnlNoDocTypes.Visibility = Visibility.Collapsed;
        chartByType.Visibility = Visibility.Visible;

        var grouped = monthDocs
            .GroupBy(d => d.Type)
            .OrderByDescending(g => g.Count())
            .Take(8)
            .ToList();

        var maxCount = grouped.Max(g => g.Count());
        var maxBarWidth = 200.0;

        var colors = new[]
        {
            "#1565C0", "#2E7D32", "#E65100", "#6A1B9A", "#00796B",
            "#C62828", "#F57F17", "#455A64"
        };

        var items = grouped.Select((g, i) => new ChartItem
        {
            TypeName = g.Key.GetDisplayName(),
            Count = g.Count(),
            BarWidth = Math.Max(20, (double)g.Count() / maxCount * maxBarWidth),
            BarColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colors[i % colors.Length]))
        }).ToList();

        chartByType.ItemsSource = items;
    }

    // ═══════════════════════════════════════
    // RECENT ACTIVITY
    // ═══════════════════════════════════════
    private void LoadRecentActivity(List<Document> allDocs)
    {
        var allMeetings = _meetingService.GetAllMeetings();

        var activities = new List<(DateTime Date, string Title, string Type)>();

        // Documents by ModifiedDate or CreatedDate
        foreach (var doc in allDocs.OrderByDescending(d => d.ModifiedDate ?? d.CreatedDate).Take(20))
        {
            var date = doc.ModifiedDate ?? doc.CreatedDate;
            var action = doc.ModifiedDate.HasValue ? "Sửa" : "Tạo";
            var number = !string.IsNullOrWhiteSpace(doc.Number) ? $"{doc.Number} — " : "";
            activities.Add((date, $"{action}: {number}{doc.Title}", "doc"));
        }

        // Meetings
        foreach (var meeting in allMeetings.OrderByDescending(m => m.ModifiedDate ?? m.CreatedDate).Take(10))
        {
            var date = meeting.ModifiedDate ?? meeting.CreatedDate;
            activities.Add((date, $"Họp: {meeting.Title}", "meeting"));
        }

        var sorted = activities.OrderByDescending(a => a.Date).Take(8).ToList();

        if (sorted.Count == 0)
        {
            pnlNoActivity.Visibility = Visibility.Visible;
            recentActivityList.Visibility = Visibility.Collapsed;
            return;
        }

        pnlNoActivity.Visibility = Visibility.Collapsed;
        recentActivityList.Visibility = Visibility.Visible;

        var items = sorted.Select(a => new ActivityItem
        {
            Title = a.Title,
            TimeAgo = FormatTimeAgo(a.Date),
            IconKind = a.Type == "doc" ? "FileDocument" : "CalendarClock",
            IconBg = a.Type == "doc"
                ? new SolidColorBrush(Color.FromRgb(227, 242, 253))
                : new SolidColorBrush(Color.FromRgb(224, 242, 241)),
            IconFg = a.Type == "doc"
                ? new SolidColorBrush(Color.FromRgb(21, 101, 192))
                : new SolidColorBrush(Color.FromRgb(0, 121, 107))
        }).ToList();

        recentActivityList.ItemsSource = items;
    }

    private static string FormatTimeAgo(DateTime date)
    {
        var diff = DateTime.Now - date;
        if (diff.TotalMinutes < 1) return "Vừa xong";
        if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes}p trước";
        if (diff.TotalHours < 24) return $"{(int)diff.TotalHours}g trước";
        if (diff.TotalDays < 2) return "Hôm qua";
        if (diff.TotalDays < 7) return $"{(int)diff.TotalDays} ngày trước";
        return date.ToString("dd/MM/yyyy");
    }

    // ═══════════════════════════════════════
    // TASKS
    // ═══════════════════════════════════════
    private void LoadTasks()
    {
        var allMeetings = _meetingService.GetAllMeetings();
        var now = DateTime.Now;

        var tasks = allMeetings
            .Where(m => m.Tasks != null)
            .SelectMany(m => m.Tasks!.Select(t => new { Task = t, Meeting = m }))
            .Where(x => x.Task.TaskStatus != MeetingTaskStatus.Completed &&
                        x.Task.TaskStatus != MeetingTaskStatus.Cancelled)
            .OrderBy(x => x.Task.Deadline ?? DateTime.MaxValue)
            .Take(6)
            .Select(x =>
            {
                var isOverdue = x.Task.Deadline.HasValue && x.Task.Deadline.Value < now;
                return new TaskItem
                {
                    Title = x.Task.Title ?? "Không tiêu đề",
                    DeadlineText = x.Task.Deadline.HasValue
                        ? $"Hạn: {x.Task.Deadline.Value:dd/MM/yyyy}" + (isOverdue ? " ⚠ QUÁ HẠN" : "")
                        : "Chưa có hạn",
                    DeadlineColor = isOverdue
                        ? new SolidColorBrush(Color.FromRgb(198, 40, 40))
                        : Brushes.Gray,
                    StatusIcon = isOverdue ? "AlertCircle" : "CheckboxBlankCircleOutline",
                    StatusColor = isOverdue
                        ? new SolidColorBrush(Color.FromRgb(198, 40, 40))
                        : new SolidColorBrush(Color.FromRgb(158, 158, 158)),
                    Background = isOverdue
                        ? new SolidColorBrush(Color.FromRgb(255, 235, 238))
                        : Brushes.Transparent,
                    MeetingLabel = TruncateString(x.Meeting.Title, 15)
                };
            }).ToList();

        if (tasks.Count == 0)
        {
            pnlNoTasks.Visibility = Visibility.Visible;
            taskList.Visibility = Visibility.Collapsed;
        }
        else
        {
            pnlNoTasks.Visibility = Visibility.Collapsed;
            taskList.Visibility = Visibility.Visible;
            taskList.ItemsSource = tasks;
        }
    }

    private static string TruncateString(string text, int maxLen)
    {
        if (string.IsNullOrEmpty(text)) return "";
        return text.Length <= maxLen ? text : text[..(maxLen - 1)] + "…";
    }

    // ═══════════════════════════════════════
    // TEMPLATES
    // ═══════════════════════════════════════
    private void LoadTemplates(List<DocumentTemplate> templates)
    {
        var topTemplates = templates
            .OrderByDescending(t => t.UsageCount)
            .Take(5)
            .ToList();

        if (topTemplates.Count == 0)
        {
            pnlNoTemplates.Visibility = Visibility.Visible;
            templateList.Visibility = Visibility.Collapsed;
            return;
        }

        pnlNoTemplates.Visibility = Visibility.Collapsed;
        templateList.Visibility = Visibility.Visible;

        var items = topTemplates.Select(t => new TemplateItem
        {
            Name = t.Name,
            UsageText = $"{t.UsageCount} lần",
            TemplateId = t.Id
        }).ToList();

        templateList.ItemsSource = items;
    }

    // ═══════════════════════════════════════
    // TREND CHART (6 months, pure WPF Canvas)
    // ═══════════════════════════════════════
    private void LoadTrendChart(List<Document> allDocs)
    {
        trendCanvas.Children.Clear();

        var now = DateTime.Now;

        // Prepare 6-month data
        var monthData = new List<(string Label, int Den, int Di, int NoiBo)>();
        for (int i = 5; i >= 0; i--)
        {
            var targetDate = now.AddMonths(-i);
            var mStart = new DateTime(targetDate.Year, targetDate.Month, 1);
            var mEnd = mStart.AddMonths(1).AddTicks(-1);

            var docs = allDocs.Where(d => d.IssueDate >= mStart && d.IssueDate <= mEnd).ToList();
            monthData.Add((
                $"T{targetDate.Month}",
                docs.Count(d => d.Direction == Direction.Den),
                docs.Count(d => d.Direction == Direction.Di),
                docs.Count(d => d.Direction == Direction.NoiBo)
            ));
        }

        var maxVal = Math.Max(1, monthData.Max(m => Math.Max(m.Den, Math.Max(m.Di, m.NoiBo))));

        // Draw after layout is ready
        trendCanvas.Dispatcher.BeginInvoke(new Action(() =>
        {
            var canvasWidth = trendCanvas.ActualWidth;
            var canvasHeight = trendCanvas.ActualHeight;
            if (canvasWidth < 100 || canvasHeight < 50) return;

            var leftMargin = 35.0;
            var bottomMargin = 25.0;
            var chartWidth = canvasWidth - leftMargin - 20;
            var chartHeight = canvasHeight - bottomMargin - 10;
            var barGroupWidth = chartWidth / 6;
            var barWidth = barGroupWidth / 4.5;

            // Y-axis grid lines
            for (int i = 0; i <= 4; i++)
            {
                var y = 10 + chartHeight - (chartHeight * i / 4);
                var line = new Line
                {
                    X1 = leftMargin, Y1 = y,
                    X2 = canvasWidth - 20, Y2 = y,
                    Stroke = Brushes.LightGray,
                    StrokeThickness = 0.5,
                    StrokeDashArray = new DoubleCollection { 4, 2 }
                };
                trendCanvas.Children.Add(line);

                var label = new TextBlock
                {
                    Text = ((int)(maxVal * i / 4.0)).ToString(),
                    FontSize = 10,
                    Foreground = Brushes.Gray
                };
                Canvas.SetLeft(label, 2);
                Canvas.SetTop(label, y - 7);
                trendCanvas.Children.Add(label);
            }

            // Bars
            var colors = new[]
            {
                Color.FromRgb(21, 101, 192),  // Den - Blue
                Color.FromRgb(76, 175, 80),   // Di - Green
                Color.FromRgb(156, 39, 176)   // NoiBo - Purple
            };

            for (int m = 0; m < 6; m++)
            {
                var data = monthData[m];
                var values = new[] { data.Den, data.Di, data.NoiBo };
                var groupX = leftMargin + m * barGroupWidth + barGroupWidth * 0.15;

                for (int b = 0; b < 3; b++)
                {
                    var barHeight = maxVal > 0 ? (double)values[b] / maxVal * chartHeight : 0;
                    if (barHeight < 2 && values[b] > 0) barHeight = 2;

                    var rect = new Rectangle
                    {
                        Width = barWidth,
                        Height = barHeight,
                        Fill = new SolidColorBrush(colors[b]),
                        RadiusX = 2,
                        RadiusY = 2,
                        Opacity = 0.85
                    };
                    Canvas.SetLeft(rect, groupX + b * (barWidth + 2));
                    Canvas.SetTop(rect, 10 + chartHeight - barHeight);
                    trendCanvas.Children.Add(rect);
                }

                // Month label
                var monthLabel = new TextBlock
                {
                    Text = data.Label,
                    FontSize = 11,
                    Foreground = Brushes.Gray,
                    TextAlignment = TextAlignment.Center
                };
                Canvas.SetLeft(monthLabel, groupX + barWidth * 0.5);
                Canvas.SetTop(monthLabel, canvasHeight - bottomMargin + 5);
                trendCanvas.Children.Add(monthLabel);
            }
        }), System.Windows.Threading.DispatcherPriority.Loaded);
    }

    // ═══════════════════════════════════════
    // NAVIGATION LINKS (stat cards + section headers)
    // ═══════════════════════════════════════
    private void StatCard_Documents_Click(object sender, MouseButtonEventArgs e)
    {
        var mainWindow = GetMainWindow();
        if (mainWindow != null)
            mainWindow.MainFrame.Navigate(new DocumentListPage(_documentService));
    }

    private void StatCard_Meetings_Click(object sender, MouseButtonEventArgs e)
    {
        var mainWindow = GetMainWindow();
        if (mainWindow != null)
            mainWindow.MainFrame.Navigate(new MeetingListPage(_documentService));
    }

    private void LinkToDocuments_Click(object sender, MouseButtonEventArgs e)
    {
        var mainWindow = GetMainWindow();
        if (mainWindow != null)
            mainWindow.MainFrame.Navigate(new DocumentListPage(_documentService));
    }

    private void LinkToMeetings_Click(object sender, MouseButtonEventArgs e)
    {
        var mainWindow = GetMainWindow();
        if (mainWindow != null)
            mainWindow.MainFrame.Navigate(new MeetingListPage(_documentService));
    }

    private void LinkToTemplates_Click(object sender, MouseButtonEventArgs e)
    {
        var mainWindow = GetMainWindow();
        if (mainWindow != null)
            mainWindow.MainFrame.Navigate(new TemplateManagementPage(_documentService));
    }

    private void LinkToStatistics_Click(object sender, MouseButtonEventArgs e)
    {
        var mainWindow = GetMainWindow();
        if (mainWindow != null)
            mainWindow.MainFrame.Navigate(new StatisticsPage(_documentService));
    }

    // ═══════════════════════════════════════
    // QUICK ACTIONS (delegate to MainWindow)
    // ═══════════════════════════════════════
    private MainWindow? GetMainWindow() => Window.GetWindow(this) as MainWindow;

    private void QuickAddDocument_Click(object sender, RoutedEventArgs e)
    {
        // Navigate to DocumentListPage
        var mainWindow = GetMainWindow();
        if (mainWindow != null)
        {
            var page = new DocumentListPage(_documentService);
            mainWindow.MainFrame.Navigate(page);
        }
    }

    private void QuickAICreate_Click(object sender, RoutedEventArgs e)
    {
        var mainWindow = GetMainWindow();
        if (mainWindow == null) return;
        if (!AiPromoHelper.CheckOrShowPromo(mainWindow)) return;
        mainWindow.MainFrame.Navigate(new AIGeneratorPage(_documentService));
    }

    private void QuickScanOCR_Click(object sender, RoutedEventArgs e)
    {
        var mainWindow = GetMainWindow();
        if (mainWindow == null) return;
        if (!AiPromoHelper.CheckOrShowPromo(mainWindow)) return;
        var dialog = new ScanImportDialog(_documentService);
        dialog.Owner = mainWindow;
        dialog.ShowDialog();
    }

    private void QuickAIReport_Click(object sender, RoutedEventArgs e)
    {
        var mainWindow = GetMainWindow();
        if (mainWindow == null) return;
        if (!AiPromoHelper.CheckOrShowPromo(mainWindow)) return;
        var dialog = new PeriodicReportDialog(_documentService);
        dialog.Owner = mainWindow;
        dialog.ShowDialog();
    }

    private void QuickAddMeeting_Click(object sender, RoutedEventArgs e)
    {
        var mainWindow = GetMainWindow();
        if (mainWindow != null)
        {
            mainWindow.MainFrame.Navigate(new MeetingListPage(_documentService));
        }
    }

    private void QuickTemplateStore_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new TemplateStoreDialog(_documentService);
        dialog.Owner = GetMainWindow();
        dialog.ShowDialog();
    }

    private void Template_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.DataContext is TemplateItem templateItem)
        {
            // Navigate to AI Generator with this template
            var mainWindow = GetMainWindow();
            if (mainWindow != null)
            {
                mainWindow.MainFrame.Navigate(new AIGeneratorPage(_documentService));
            }
        }
    }
}

// ═══════════════════════════════════════════════════════════
// Schedule view models (top-level for XAML DataTemplateSelector)
// ═══════════════════════════════════════════════════════════

public class ScheduleDayHeader
{
    public string DayLabel { get; set; } = "";
    public string DateText { get; set; } = "";
    public int MeetingCount { get; set; }
}

public class ScheduleMeetingItem
{
    public string Time { get; set; } = "";
    public string Title { get; set; } = "";
    public string Location { get; set; } = "";
    public string ChairPerson { get; set; } = "";
    public string FormatText { get; set; } = "";
    public Brush FormatBg { get; set; } = Brushes.Transparent;
    public Brush FormatFg { get; set; } = Brushes.White;
    public Brush PriorityBrush { get; set; } = Brushes.Gray;
    public int TaskCount { get; set; }
    public int AttendeeCount { get; set; }
    public string StatusText { get; set; } = "";
    public Brush StatusBrush { get; set; } = Brushes.Gray;
    public Visibility LocationVisible { get; set; } = Visibility.Collapsed;
    public Visibility ChairVisible { get; set; } = Visibility.Collapsed;
    public Visibility TaskVisible { get; set; } = Visibility.Collapsed;
}

public class ScheduleTemplateSelector : DataTemplateSelector
{
    public DataTemplate? DayHeaderTemplate { get; set; }
    public DataTemplate? MeetingItemTemplate { get; set; }

    public override DataTemplate? SelectTemplate(object item, DependencyObject container)
    {
        if (item is ScheduleDayHeader) return DayHeaderTemplate;
        if (item is ScheduleMeetingItem) return MeetingItemTemplate;
        return base.SelectTemplate(item, container);
    }
}
