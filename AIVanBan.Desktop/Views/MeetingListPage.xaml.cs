using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using AIVanBan.Core.Models;
using AIVanBan.Core.Services;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;

namespace AIVanBan.Desktop.Views;

public partial class MeetingListPage : Page
{
    private readonly MeetingService _meetingService;
    private readonly DocumentService _documentService;
    private readonly MeetingWordExportService _exportService = new();
    private List<Meeting> _currentMeetings = new();
    private bool _isLoading = true; // Block filter events during initialization
    
    // Debounce timer cho tìm kiếm realtime
    private readonly DispatcherTimer _searchDebounceTimer;
    
    public MeetingListPage(DocumentService documentService)
    {
        InitializeComponent();
        _meetingService = new MeetingService();
        _documentService = documentService;
        
        // Khởi tạo debounce timer (300ms) cho tìm kiếm realtime
        _searchDebounceTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(300)
        };
        _searchDebounceTimer.Tick += (s, e) =>
        {
            _searchDebounceTimer.Stop();
            if (!_isLoading) LoadMeetings();
        };
        
        InitializeFilters();
        
        // Load data after XAML is fully rendered
        Loaded += MeetingListPage_Loaded;
    }
    
    private void MeetingListPage_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            _isLoading = false; // Now allow filter events
            
            // Auto refresh overdue tasks on load
            _meetingService.RefreshOverdueTasks();
            LoadMeetings();
            LoadStatistics();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi tải danh sách cuộc họp:\n{ex.Message}\n\n{ex.StackTrace}", "Lỗi",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    /// <summary>
    /// Khởi tạo combo box loại cuộc họp
    /// </summary>
    private void InitializeFilters()
    {
        // Populate MeetingType combo
        foreach (MeetingType type in Enum.GetValues(typeof(MeetingType)))
        {
            cboType.Items.Add(new ComboBoxItem
            {
                Content = MeetingHelper.GetTypeName(type),
                Tag = type.ToString()
            });
        }
    }
    
    #region Load & Display
    
    private void LoadMeetings()
    {
        _isLoading = true;
        
        try
        {
            // Get filter values
            string? keyword = string.IsNullOrWhiteSpace(txtSearch.Text) ? null : txtSearch.Text.Trim();
            
            MeetingType? typeFilter = null;
            if (cboType.SelectedItem is ComboBoxItem typeItem && !string.IsNullOrEmpty(typeItem.Tag?.ToString()))
            {
                if (Enum.TryParse<MeetingType>(typeItem.Tag.ToString(), out var t))
                    typeFilter = t;
            }
            
            MeetingStatus? statusFilter = null;
            if (cboStatus.SelectedItem is ComboBoxItem statusItem && !string.IsNullOrEmpty(statusItem.Tag?.ToString()))
            {
                if (Enum.TryParse<MeetingStatus>(statusItem.Tag.ToString(), out var s))
                    statusFilter = s;
            }
            
            DateTime? fromDate = dpFrom.SelectedDate;
            DateTime? toDate = dpTo.SelectedDate;
            
            // Filter
            _currentMeetings = _meetingService.FilterMeetings(keyword, typeFilter, statusFilter, fromDate, toDate);
            
            // Display
            DisplayMeetings(_currentMeetings);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi tải danh sách cuộc họp:\n{ex.Message}\n\n{ex.StackTrace}", "Lỗi",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            _isLoading = false;
        }
    }
    
    private void DisplayMeetings(List<Meeting> meetings)
    {
        meetingListPanel.Children.Clear();
        
        if (meetings.Count == 0)
        {
            emptyState.Visibility = Visibility.Visible;
            return;
        }
        
        emptyState.Visibility = Visibility.Collapsed;
        
        // Group meetings by date
        var grouped = meetings.GroupBy(m => m.StartTime.Date).OrderByDescending(g => g.Key);
        
        foreach (var group in grouped)
        {
            // Date header
            var dateHeader = CreateDateHeader(group.Key);
            meetingListPanel.Children.Add(dateHeader);
            
            // Meeting cards for this date
            foreach (var meeting in group.OrderBy(m => m.StartTime))
            {
                var card = CreateMeetingCard(meeting);
                meetingListPanel.Children.Add(card);
            }
        }
    }
    
    /// <summary>
    /// Tạo header ngày (nhóm theo ngày)
    /// </summary>
    private UIElement CreateDateHeader(DateTime date)
    {
        var isToday = date.Date == DateTime.Today;
        var isTomorrow = date.Date == DateTime.Today.AddDays(1);
        var isYesterday = date.Date == DateTime.Today.AddDays(-1);
        
        string dateText;
        if (isToday) dateText = $"📍 HÔM NAY — {MeetingHelper.FormatMeetingDate(date)}";
        else if (isTomorrow) dateText = $"⏭️ NGÀY MAI — {MeetingHelper.FormatMeetingDate(date)}";
        else if (isYesterday) dateText = $"◀️ HÔM QUA — {MeetingHelper.FormatMeetingDate(date)}";
        else dateText = $"📅 {MeetingHelper.FormatMeetingDate(date)}";
        
        var header = new TextBlock
        {
            Text = dateText,
            FontSize = 14,
            FontWeight = isToday ? FontWeights.Bold : FontWeights.SemiBold,
            Foreground = isToday ? Brushes.White : new SolidColorBrush(Color.FromRgb(0x42, 0x42, 0x42)),
            Margin = new Thickness(0, 16, 0, 8),
            Padding = new Thickness(12, 6, 12, 6)
        };
        
        if (isToday)
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(0x19, 0x76, 0xD2)),
                CornerRadius = new CornerRadius(6),
                Margin = new Thickness(0, 16, 0, 8),
                Child = header
            };
            header.Margin = new Thickness(0);
            return border;
        }
        
        return header;
    }
    
    /// <summary>
    /// Tạo card hiển thị cuộc họp — với trạng thái trực quan, relative time, hover effects
    /// </summary>
    private UIElement CreateMeetingCard(Meeting meeting)
    {
        // Xác định trạng thái hiện tại
        var now = DateTime.Now;
        bool isHappeningNow = meeting.Status == MeetingStatus.InProgress ||
            (meeting.StartTime <= now && (meeting.EndTime == null || meeting.EndTime.Value >= now) 
             && meeting.Status == MeetingStatus.Scheduled);
        bool isToday = meeting.StartTime.Date == DateTime.Today;
        
        // B4: Card background tint theo status
        var cardBg = meeting.Status switch
        {
            MeetingStatus.Completed => Color.FromRgb(0xF9, 0xFB, 0xF9),  // Light green
            MeetingStatus.Cancelled => Color.FromRgb(0xFA, 0xFA, 0xFA),  // Light gray
            MeetingStatus.Postponed => Color.FromRgb(0xFF, 0xFB, 0xF0),  // Light orange
            _ when isHappeningNow => Color.FromRgb(0xFF, 0xF5, 0xF5),    // Light red for live
            _ when isToday => Color.FromRgb(0xF5, 0xF9, 0xFF),           // Light blue for today
            _ => Color.FromRgb(0xFF, 0xFF, 0xFF)                         // White default
        };
        
        var card = new Card
        {
            Margin = new Thickness(0, 0, 0, 8),
            Padding = new Thickness(0),
            UniformCornerRadius = 8,
            Tag = meeting.Id,
            Cursor = Cursors.Hand,
            Background = new SolidColorBrush(cardBg)
        };
        
        // B4: Hover effect
        var defaultBg = cardBg;
        card.MouseEnter += (s, e) =>
        {
            card.Background = new SolidColorBrush(Color.FromRgb(
                (byte)Math.Max(0, defaultBg.R - 8),
                (byte)Math.Max(0, defaultBg.G - 4),
                (byte)Math.Max(0, defaultBg.B - 2)));
            ElevationAssist.SetElevation(card, Elevation.Dp4);
        };
        card.MouseLeave += (s, e) =>
        {
            card.Background = new SolidColorBrush(defaultBg);
            ElevationAssist.SetElevation(card, Elevation.Dp1);
        };
        
        // Outer border with left color strip
        var outerGrid = new Grid();
        outerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(meeting.Priority >= 4 ? 8 : 5) }); // B4: wider strip for high priority
        outerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        
        // Color strip based on status
        var statusColor = MeetingHelper.GetStatusColor(meeting.Status);
        var stripColor = isHappeningNow ? "#E53935" : statusColor; // Red for live meetings
        var colorStrip = new Border
        {
            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(stripColor)),
            CornerRadius = new CornerRadius(8, 0, 0, 8)
        };
        Grid.SetColumn(colorStrip, 0);
        outerGrid.Children.Add(colorStrip);
        
        // Main content
        var mainGrid = new Grid { Margin = new Thickness(16, 12, 16, 12) };
        mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });  // Time
        mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Content
        mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Actions
        Grid.SetColumn(mainGrid, 1);
        outerGrid.Children.Add(mainGrid);
        
        // === Column 0: Time + Relative time ===
        var timePanel = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
        
        // B2: "ĐANG DIỄN RA" badge
        if (isHappeningNow)
        {
            var liveBadge = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(0xE5, 0x39, 0x35)),
                CornerRadius = new CornerRadius(3),
                Padding = new Thickness(4, 1, 4, 1),
                Margin = new Thickness(0, 0, 0, 3),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            liveBadge.Child = new TextBlock
            {
                Text = "● LIVE",
                FontSize = 8,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White
            };
            timePanel.Children.Add(liveBadge);
        }
        
        var timeText = new TextBlock
        {
            Text = meeting.IsAllDay ? "Cả ngày" : meeting.StartTime.ToString("HH:mm"),
            FontSize = meeting.IsAllDay ? 13 : 20,
            FontWeight = FontWeights.Bold,
            Foreground = isHappeningNow 
                ? new SolidColorBrush(Color.FromRgb(0xE5, 0x39, 0x35))
                : new SolidColorBrush(Color.FromRgb(0x19, 0x76, 0xD2)),
            HorizontalAlignment = HorizontalAlignment.Center
        };
        timePanel.Children.Add(timeText);
        
        if (!meeting.IsAllDay && meeting.EndTime.HasValue)
        {
            var endTimeText = new TextBlock
            {
                Text = meeting.EndTime.Value.ToString("HH:mm"),
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(0x75, 0x75, 0x75)),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            timePanel.Children.Add(endTimeText);
        }
        
        // B2: Relative time label
        var relativeTime = GetRelativeTimeText(meeting);
        if (!string.IsNullOrEmpty(relativeTime))
        {
            var relColor = isHappeningNow ? Color.FromRgb(0xE5, 0x39, 0x35) :
                meeting.StartTime > now ? Color.FromRgb(0xE6, 0x51, 0x00) :
                Color.FromRgb(0x9E, 0x9E, 0x9E);
            timePanel.Children.Add(new TextBlock
            {
                Text = relativeTime,
                FontSize = 9,
                FontWeight = isHappeningNow ? FontWeights.Bold : FontWeights.Normal,
                Foreground = new SolidColorBrush(relColor),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 2, 0, 0),
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            });
        }
        
        Grid.SetColumn(timePanel, 0);
        mainGrid.Children.Add(timePanel);
        
        // === Column 1: Content ===
        var contentPanel = new StackPanel { Margin = new Thickness(12, 0, 12, 0), VerticalAlignment = VerticalAlignment.Center };
        
        // Title row
        var titlePanel = new WrapPanel { Orientation = Orientation.Horizontal };
        
        // Priority indicator
        if (meeting.Priority >= 4)
        {
            var priorityIcon = new PackIcon
            {
                Kind = PackIconKind.AlertCircle,
                Width = 16, Height = 16,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(MeetingHelper.GetPriorityColor(meeting.Priority))),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 4, 0)
            };
            titlePanel.Children.Add(priorityIcon);
        }
        
        var titleBlock = new TextBlock
        {
            Text = meeting.Title,
            FontSize = 15,
            FontWeight = FontWeights.SemiBold,
            TextTrimming = TextTrimming.CharacterEllipsis,
            MaxWidth = 500,
            // B4: Strikethrough for cancelled meetings
            TextDecorations = meeting.Status == MeetingStatus.Cancelled ? TextDecorations.Strikethrough : null,
            Foreground = meeting.Status == MeetingStatus.Cancelled 
                ? new SolidColorBrush(Color.FromRgb(0x9E, 0x9E, 0x9E))
                : new SolidColorBrush(Color.FromRgb(0x21, 0x21, 0x21))
        };
        titlePanel.Children.Add(titleBlock);
        contentPanel.Children.Add(titlePanel);
        
        // Info row: Location + Format
        var infoPanel = new WrapPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 4, 0, 0) };
        
        if (!string.IsNullOrEmpty(meeting.Location))
        {
            var locIcon = new PackIcon { Kind = PackIconKind.MapMarker, Width = 14, Height = 14, 
                Foreground = new SolidColorBrush(Color.FromRgb(0x75, 0x75, 0x75)), VerticalAlignment = VerticalAlignment.Center };
            infoPanel.Children.Add(locIcon);
            infoPanel.Children.Add(new TextBlock { Text = meeting.Location, FontSize = 12, 
                Foreground = new SolidColorBrush(Color.FromRgb(0x61, 0x61, 0x61)), Margin = new Thickness(3, 0, 12, 0),
                VerticalAlignment = VerticalAlignment.Center, MaxWidth = 250, TextTrimming = TextTrimming.CharacterEllipsis });
        }
        
        // Format badge
        var formatBadge = CreateBadge(
            $"{MeetingHelper.GetFormatIcon(meeting.Format)} {MeetingHelper.GetFormatName(meeting.Format)}",
            meeting.Format == MeetingFormat.TrucTuyen ? "#E3F2FD" : 
            meeting.Format == MeetingFormat.KetHop ? "#FFF3E0" : "#E8F5E9",
            meeting.Format == MeetingFormat.TrucTuyen ? "#1565C0" :
            meeting.Format == MeetingFormat.KetHop ? "#E65100" : "#2E7D32");
        infoPanel.Children.Add(formatBadge);
        
        contentPanel.Children.Add(infoPanel);
        
        // Chair person + Organizing unit
        var peoplePanel = new WrapPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 3, 0, 0) };
        
        if (!string.IsNullOrEmpty(meeting.ChairPerson))
        {
            var chairIcon = new PackIcon { Kind = PackIconKind.Account, Width = 14, Height = 14, 
                Foreground = new SolidColorBrush(Color.FromRgb(0x75, 0x75, 0x75)), VerticalAlignment = VerticalAlignment.Center };
            peoplePanel.Children.Add(chairIcon);
            
            var chairText = meeting.ChairPerson;
            if (!string.IsNullOrEmpty(meeting.ChairPersonTitle))
                chairText += $" - {meeting.ChairPersonTitle}";
            
            peoplePanel.Children.Add(new TextBlock { Text = chairText, FontSize = 12, 
                Foreground = new SolidColorBrush(Color.FromRgb(0x61, 0x61, 0x61)), 
                Margin = new Thickness(3, 0, 12, 0), VerticalAlignment = VerticalAlignment.Center });
        }
        
        if (!string.IsNullOrEmpty(meeting.OrganizingUnit))
        {
            var orgIcon = new PackIcon { Kind = PackIconKind.Domain, Width = 14, Height = 14, 
                Foreground = new SolidColorBrush(Color.FromRgb(0x75, 0x75, 0x75)), VerticalAlignment = VerticalAlignment.Center };
            peoplePanel.Children.Add(orgIcon);
            peoplePanel.Children.Add(new TextBlock { Text = meeting.OrganizingUnit, FontSize = 12, 
                Foreground = new SolidColorBrush(Color.FromRgb(0x61, 0x61, 0x61)), 
                Margin = new Thickness(3, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center });
        }
        
        if (peoplePanel.Children.Count > 0)
            contentPanel.Children.Add(peoplePanel);
        
        // Tags row: Type badge + Status badge + Task progress
        var tagsPanel = new WrapPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 6, 0, 0) };
        
        // Type badge
        var typeBadge = CreateBadge(MeetingHelper.GetTypeName(meeting.Type), "#F3E5F5", "#6A1B9A");
        tagsPanel.Children.Add(typeBadge);
        
        // Status badge
        var statusBadge = CreateBadge(
            $"{MeetingHelper.GetStatusIcon(meeting.Status)} {MeetingHelper.GetStatusName(meeting.Status)}",
            meeting.Status == MeetingStatus.Completed ? "#E8F5E9" :
            meeting.Status == MeetingStatus.Scheduled ? "#E3F2FD" :
            meeting.Status == MeetingStatus.InProgress ? "#FFEBEE" :
            meeting.Status == MeetingStatus.Postponed ? "#FFF3E0" : "#F5F5F5",
            MeetingHelper.GetStatusColor(meeting.Status));
        tagsPanel.Children.Add(statusBadge);
        
        // Level badge
        var levelBadge = CreateBadge(MeetingHelper.GetLevelName(meeting.Level), "#E0F7FA", "#00695C");
        tagsPanel.Children.Add(levelBadge);
        
        // Task progress
        if (meeting.Tasks != null && meeting.Tasks.Count > 0)
        {
            var completedTasks = meeting.Tasks.Count(t => t.TaskStatus == MeetingTaskStatus.Completed);
            var totalTasks = meeting.Tasks.Count;
            var overdueTasks = meeting.Tasks.Count(t => t.TaskStatus == MeetingTaskStatus.Overdue ||
                ((t.TaskStatus == MeetingTaskStatus.NotStarted || t.TaskStatus == MeetingTaskStatus.InProgress) &&
                 t.Deadline.HasValue && t.Deadline.Value < DateTime.Now));
            
            var taskColor = overdueTasks > 0 ? "#E53935" : completedTasks == totalTasks ? "#43A047" : "#1976D2";
            var taskBg = overdueTasks > 0 ? "#FFEBEE" : completedTasks == totalTasks ? "#E8F5E9" : "#E3F2FD";
            var taskText = $"📌 {completedTasks}/{totalTasks} NV";
            if (overdueTasks > 0) taskText += $" ({overdueTasks} quá hạn)";
            
            var taskBadge = CreateBadge(taskText, taskBg, taskColor);
            tagsPanel.Children.Add(taskBadge);
            
            // B2: Mini progress bar cho nhiệm vụ
            var progressRatio = (double)completedTasks / totalTasks;
            var progressBar = new Border
            {
                Width = 50,
                Height = 4,
                CornerRadius = new CornerRadius(2),
                Background = new SolidColorBrush(Color.FromRgb(0xE0, 0xE0, 0xE0)),
                Margin = new Thickness(0, 0, 6, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Child = new Border
                {
                    Width = 50 * progressRatio,
                    Height = 4,
                    CornerRadius = new CornerRadius(2),
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(taskColor)),
                    HorizontalAlignment = HorizontalAlignment.Left
                }
            };
            tagsPanel.Children.Add(progressBar);
        }
        
        // Attendees count
        if (meeting.Attendees != null && meeting.Attendees.Count > 0)
        {
            var attendeeBadge = CreateBadge($"👥 {meeting.Attendees.Count} người", "#ECEFF1", "#455A64");
            tagsPanel.Children.Add(attendeeBadge);
        }
        
        // Documents count + invitation warning
        if (meeting.Documents != null && meeting.Documents.Count > 0)
        {
            var hasInvitation = meeting.Documents.Any(d => d.DocumentType == MeetingDocumentType.GiayMoi);
            var docBadge = CreateBadge($"📄 {meeting.Documents.Count} VB", hasInvitation ? "#E8EAF6" : "#FFEBEE", hasInvitation ? "#283593" : "#C62828");
            tagsPanel.Children.Add(docBadge);
        }
        else
        {
            // Warning: no documents at all
            var noDocBadge = CreateBadge("⚠️ Chưa có VB", "#FFEBEE", "#C62828");
            tagsPanel.Children.Add(noDocBadge);
        }
        
        // Album count badge
        if (meeting.RelatedAlbumIds != null && meeting.RelatedAlbumIds.Length > 0)
        {
            var albumBadge = CreateBadge($"📷 {meeting.RelatedAlbumIds.Length} album", "#FCE4EC", "#AD1457");
            albumBadge.Cursor = Cursors.Hand;
            albumBadge.ToolTip = "Nhấn để xem album ảnh liên quan";
            var capturedMeeting = meeting;
            albumBadge.MouseLeftButtonUp += (s, ev) =>
            {
                ev.Handled = true;
                OpenMeetingAlbumTab(capturedMeeting);
            };
            tagsPanel.Children.Add(albumBadge);
        }
        
        contentPanel.Children.Add(tagsPanel);
        
        // Personal notes preview
        if (!string.IsNullOrEmpty(meeting.PersonalNotes))
        {
            var notePreview = new TextBlock
            {
                Text = $"📝 {meeting.PersonalNotes.Split('\n')[0]}",
                FontSize = 11,
                FontStyle = FontStyles.Italic,
                Foreground = new SolidColorBrush(Color.FromRgb(0x9E, 0x9E, 0x9E)),
                Margin = new Thickness(0, 4, 0, 0),
                TextTrimming = TextTrimming.CharacterEllipsis,
                MaxWidth = 400
            };
            contentPanel.Children.Add(notePreview);
        }
        
        Grid.SetColumn(contentPanel, 1);
        mainGrid.Children.Add(contentPanel);
        
        // === Column 2: Actions ===
        var actionPanel = new StackPanel { VerticalAlignment = VerticalAlignment.Center, Orientation = Orientation.Horizontal };
        
        // Quick status toggle
        if (meeting.Status == MeetingStatus.Scheduled)
        {
            var btnStart = new Button
            {
                Style = (Style)FindResource("MaterialDesignIconButton"),
                ToolTip = "✅ Đánh dấu Đã kết thúc",
                Tag = meeting.Id,
                Foreground = new SolidColorBrush(Color.FromRgb(0x43, 0xA0, 0x47)),
                Width = 36, Height = 36
            };
            btnStart.Content = new PackIcon { Kind = PackIconKind.CheckCircleOutline, Width = 18, Height = 18 };
            btnStart.Click += (s, _) => QuickChangeStatus(meeting.Id, MeetingStatus.Completed);
            actionPanel.Children.Add(btnStart);
        }
        else if (meeting.Status == MeetingStatus.InProgress)
        {
            var btnComplete = new Button
            {
                Style = (Style)FindResource("MaterialDesignIconButton"),
                ToolTip = "✅ Đánh dấu Đã kết thúc",
                Tag = meeting.Id,
                Foreground = new SolidColorBrush(Color.FromRgb(0x43, 0xA0, 0x47)),
                Width = 36, Height = 36
            };
            btnComplete.Content = new PackIcon { Kind = PackIconKind.CheckCircle, Width = 18, Height = 18 };
            btnComplete.Click += (s, _) => QuickChangeStatus(meeting.Id, MeetingStatus.Completed);
            actionPanel.Children.Add(btnComplete);
        }
        
        // Export Word button with dropdown
        var btnExport = new Button
        {
            Style = (Style)FindResource("MaterialDesignIconButton"),
            ToolTip = "Xuất Word",
            Tag = meeting.Id,
            Foreground = new SolidColorBrush(Color.FromRgb(0x19, 0x76, 0xD2)),
            Width = 36, Height = 36
        };
        btnExport.Content = new PackIcon { Kind = PackIconKind.FileWord, Width = 18, Height = 18 };
        btnExport.Click += (s, _) => ShowExportMenu(meeting, btnExport);
        actionPanel.Children.Add(btnExport);
        
        var btnEdit = new Button
        {
            Style = (Style)FindResource("MaterialDesignIconButton"),
            ToolTip = "Xem / Sửa",
            Tag = meeting.Id,
            Width = 36, Height = 36
        };
        btnEdit.Content = new PackIcon { Kind = PackIconKind.Pencil, Width = 18, Height = 18 };
        btnEdit.Click += EditMeeting_Click;
        actionPanel.Children.Add(btnEdit);
        
        // More actions button (duplicate, delete, status change...)
        var btnMore = new Button
        {
            Style = (Style)FindResource("MaterialDesignIconButton"),
            ToolTip = "Thêm tùy chọn",
            Tag = meeting.Id,
            Width = 36, Height = 36
        };
        btnMore.Content = new PackIcon { Kind = PackIconKind.DotsVertical, Width = 18, Height = 18 };
        btnMore.Click += (s, _) => ShowMoreMenu(meeting, btnMore);
        actionPanel.Children.Add(btnMore);
        
        Grid.SetColumn(actionPanel, 2);
        mainGrid.Children.Add(actionPanel);
        
        card.Content = outerGrid;
        
        // Double-click to edit
        card.MouseLeftButtonDown += (s, e) =>
        {
            if (e.ClickCount == 2)
            {
                OpenEditDialog(meeting.Id);
                e.Handled = true;
            }
        };
        
        return card;
    }
    
    /// <summary>
    /// Tạo badge nhỏ (chip-like)
    /// </summary>
    private Border CreateBadge(string text, string bgColor, string fgColor)
    {
        return new Border
        {
            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(bgColor)),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(8, 2, 8, 2),
            Margin = new Thickness(0, 0, 6, 0),
            Child = new TextBlock
            {
                Text = text,
                FontSize = 11,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(fgColor)),
                VerticalAlignment = VerticalAlignment.Center
            }
        };
    }
    
    /// <summary>
    /// B2: Tính thời gian tương đối cho cuộc họp (VD: "Bây giờ", "Sau 2 giờ", "Ngày mai")
    /// </summary>
    private static string GetRelativeTimeText(Meeting meeting)
    {
        var now = DateTime.Now;
        var diff = meeting.StartTime - now;
        var endTime = meeting.EndTime ?? meeting.StartTime.AddHours(1);
        
        // Đang diễn ra
        if (meeting.StartTime <= now && endTime >= now)
            return "Bây giờ";
        
        // Đã qua
        if (diff.TotalMinutes < 0)
        {
            if (meeting.StartTime.Date == DateTime.Today)
            {
                var elapsed = Math.Abs((int)diff.TotalHours);
                return elapsed < 1 ? "Vừa qua" : $"{elapsed}h trước";
            }
            if (meeting.StartTime.Date == DateTime.Today.AddDays(-1))
                return "Hôm qua";
            if (Math.Abs(diff.TotalDays) < 7)
                return $"{Math.Abs((int)diff.TotalDays)} ngày trước";
            return ""; // Quá xa, không hiện
        }
        
        // Sắp tới
        if (diff.TotalMinutes <= 30)
            return $"Sau {Math.Max(1, (int)diff.TotalMinutes)}p";
        if (diff.TotalHours < 1)
            return "Sau 30p";
        if (meeting.StartTime.Date == DateTime.Today)
            return $"Sau {(int)diff.TotalHours}h";
        if (meeting.StartTime.Date == DateTime.Today.AddDays(1))
            return "Ngày mai";
        if (diff.TotalDays < 7)
            return $"Còn {(int)diff.TotalDays} ngày";
        return ""; // Quá xa, không hiện
    }
    
    private void LoadStatistics()
    {
        try
        {
            txtStatTotal.Text = _meetingService.GetTotalMeetings().ToString();
            txtStatMonth.Text = _meetingService.GetMeetingsCountThisMonth().ToString();
            txtStatUpcoming.Text = _meetingService.GetUpcomingCount().ToString();
            txtStatPending.Text = _meetingService.GetPendingTaskCount().ToString();
            
            var overdue = _meetingService.GetOverdueTaskCount();
            txtStatOverdue.Text = overdue.ToString();
            
            // Highlight overdue card nếu có NV quá hạn
            if (overdue > 0)
            {
                cardOverdue.Background = new SolidColorBrush(Color.FromRgb(0xFF, 0xEB, 0xEE));
                cardOverdue.BorderBrush = new SolidColorBrush(Color.FromRgb(0xE5, 0x39, 0x35));
                txtOverdueLabel.Text = "QUÁ HẠN!";
                txtOverdueLabel.Foreground = new SolidColorBrush(Color.FromRgb(0xE5, 0x39, 0x35));
                txtOverdueLabel.FontWeight = FontWeights.Bold;
            }
            else
            {
                cardOverdue.Background = Brushes.White;
                cardOverdue.BorderBrush = new SolidColorBrush(Color.FromRgb(0xE0, 0xE0, 0xE0));
                txtOverdueLabel.Text = "quá hạn";
                txtOverdueLabel.Foreground = new SolidColorBrush(Color.FromRgb(0x9E, 0x9E, 0x9E));
                txtOverdueLabel.FontWeight = FontWeights.Normal;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Error loading meeting stats: {ex.Message}");
        }
    }
    
    #endregion
    
    #region Event Handlers
    
    private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            _searchDebounceTimer.Stop(); // Hủy debounce nếu đang chờ
            LoadMeetings();
        }
    }
    
    /// <summary>
    /// Tìm kiếm realtime: gõ tới đâu lọc tới đó (debounce 300ms).
    /// </summary>
    private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isLoading) return;
        _searchDebounceTimer.Stop();
        _searchDebounceTimer.Start();
    }
    
    private void Filter_Changed(object sender, object e)
    {
        if (_isLoading) return;
        LoadMeetings();
    }
    
    private void ClearFilter_Click(object sender, RoutedEventArgs e)
    {
        _isLoading = true;
        txtSearch.Text = "";
        cboType.SelectedIndex = 0;
        cboStatus.SelectedIndex = 0;
        dpFrom.SelectedDate = null;
        dpTo.SelectedDate = null;
        _isLoading = false;
        
        LoadMeetings();
    }

    // ═══════ Quick Filter Chips ═══════

    private void QuickFilter_Today(object sender, RoutedEventArgs e)
    {
        _isLoading = true;
        txtSearch.Text = "";
        cboType.SelectedIndex = 0;
        cboStatus.SelectedIndex = 0;
        dpFrom.SelectedDate = DateTime.Today;
        dpTo.SelectedDate = DateTime.Today;
        _isLoading = false;
        LoadMeetings();
    }

    private void QuickFilter_Week(object sender, RoutedEventArgs e)
    {
        _isLoading = true;
        txtSearch.Text = "";
        cboType.SelectedIndex = 0;
        cboStatus.SelectedIndex = 0;
        var today = DateTime.Today;
        // Thứ 2 đầu tuần (chuẩn VN) 
        var dayOfWeek = ((int)today.DayOfWeek + 6) % 7; // Mon=0, Sun=6
        dpFrom.SelectedDate = today.AddDays(-dayOfWeek);
        dpTo.SelectedDate = today.AddDays(6 - dayOfWeek);
        _isLoading = false;
        LoadMeetings();
    }

    private void QuickFilter_Month(object sender, RoutedEventArgs e)
    {
        _isLoading = true;
        txtSearch.Text = "";
        cboType.SelectedIndex = 0;
        cboStatus.SelectedIndex = 0;
        var today = DateTime.Today;
        dpFrom.SelectedDate = new DateTime(today.Year, today.Month, 1);
        dpTo.SelectedDate = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
        _isLoading = false;
        LoadMeetings();
    }

    private void QuickFilter_Upcoming(object sender, RoutedEventArgs e)
    {
        _isLoading = true;
        txtSearch.Text = "";
        cboType.SelectedIndex = 0;
        cboStatus.SelectedIndex = 0;
        dpFrom.SelectedDate = DateTime.Today;
        dpTo.SelectedDate = DateTime.Today.AddDays(7);
        _isLoading = false;
        LoadMeetings();
    }
    
    private void AddMeeting_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var dialog = new MeetingEditDialog(null, _meetingService, _documentService)
            {
                Owner = Window.GetWindow(this)
            };
            
            if (dialog.ShowDialog() == true)
            {
                LoadMeetings();
                LoadStatistics();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi mở dialog thêm cuộc họp:\n{ex.Message}", "Lỗi",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Tạo nhanh cuộc họp — chỉ hiện form tối thiểu (tên + thời gian + địa điểm)
    /// </summary>
    private void QuickAddMeeting_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var dialog = new MeetingEditDialog(_meetingService, _documentService, DateTime.Today)
            {
                Owner = Window.GetWindow(this)
            };

            if (dialog.ShowDialog() == true)
            {
                LoadMeetings();
                LoadStatistics();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi mở dialog tạo nhanh:\n{ex.Message}", "Lỗi",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    private void SeedDemoMeetings_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var result = MessageBox.Show(
                "Tạo dữ liệu mẫu 17 cuộc họp cho UBND xã Hòa Bình?\n\n" +
                "Bao gồm: Họp thường kỳ, giao ban, Chi bộ, HĐND, BCĐ NTM, tiếp dân, chuyên đề, tập huấn...\n\n" +
                "Dữ liệu sẽ có đầy đủ: giấy mời, tài liệu, biên bản, kết luận, nhiệm vụ.",
                "Tạo dữ liệu mẫu",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                var seeder = new MeetingSeeder(_meetingService);
                seeder.SeedDemoMeetings();
                LoadMeetings();
                LoadStatistics();
                MessageBox.Show("✅ Đã tạo dữ liệu mẫu thành công!", "Thành công",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi tạo dữ liệu mẫu:\n{ex.Message}\n\n{ex.StackTrace}", "Lỗi",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    private void EditMeeting_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string meetingId)
        {
            OpenEditDialog(meetingId);
        }
    }
    
    private void OpenEditDialog(string meetingId)
    {
        try
        {
            var meeting = _meetingService.GetMeetingById(meetingId);
            if (meeting == null)
            {
                MessageBox.Show("Không tìm thấy cuộc họp!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            var dialog = new MeetingEditDialog(meeting, _meetingService, _documentService)
            {
                Owner = Window.GetWindow(this)
            };
            
            if (dialog.ShowDialog() == true)
            {
                LoadMeetings();
                LoadStatistics();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi mở cuộc họp:\n{ex.Message}", "Lỗi",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    private void OpenMeetingAlbumTab(Meeting meeting)
    {
        try
        {
            var dialog = new MeetingEditDialog(meeting, _meetingService, _documentService)
            {
                Owner = Window.GetWindow(this)
            };
            dialog.Loaded += (s, ev) =>
            {
                // Tab 6 = Album ảnh (index 5)
                dialog.tabControl.SelectedIndex = 5;
            };
            
            if (dialog.ShowDialog() == true)
            {
                LoadMeetings();
                LoadStatistics();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi mở album cuộc họp:\n{ex.Message}", "Lỗi",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    private void DeleteMeeting_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string meetingId)
        {
            var meeting = _meetingService.GetMeetingById(meetingId);
            if (meeting == null) return;
            
            var result = MessageBox.Show(
                $"Bạn có chắc muốn xóa cuộc họp:\n\n\"{meeting.Title}\"\n\n📅 {meeting.StartTime:dd/MM/yyyy HH:mm}\n📍 {meeting.Location}\n\nThao tác này không thể hoàn tác!",
                "Xác nhận xóa",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            
            if (result == MessageBoxResult.Yes)
            {
                _meetingService.DeleteMeeting(meetingId);
                LoadMeetings();
                LoadStatistics();
            }
        }
    }
    
    // ======= EXPORT WORD =======
    
    private void ShowExportMenu(Meeting meeting, Button anchor)
    {
        var menu = new ContextMenu();
        
        var itemBienBan = new MenuItem { Header = "📝 Xuất Biên bản cuộc họp", Tag = meeting };
        itemBienBan.Click += (s, _) => ExportMeetingWord(meeting, "BienBan");
        menu.Items.Add(itemBienBan);
        
        var itemKetLuan = new MenuItem { Header = "📌 Xuất Thông báo kết luận", Tag = meeting };
        itemKetLuan.Click += (s, _) => ExportMeetingWord(meeting, "KetLuan");
        menu.Items.Add(itemKetLuan);
        
        menu.Items.Add(new Separator());
        
        var itemTongHop = new MenuItem { Header = "📋 Xuất Báo cáo tổng hợp (đầy đủ)", Tag = meeting };
        itemTongHop.Click += (s, _) => ExportMeetingWord(meeting, "TongHop");
        menu.Items.Add(itemTongHop);
        
        menu.PlacementTarget = anchor;
        menu.IsOpen = true;
    }
    
    private void ExportMeetingWord(Meeting meeting, string exportType)
    {
        try
        {
            // Tạo tên file gợi ý
            var safeTitle = string.Join("_", meeting.Title.Split(Path.GetInvalidFileNameChars())).Trim();
            if (safeTitle.Length > 60) safeTitle = safeTitle.Substring(0, 60);
            
            var prefix = exportType switch
            {
                "BienBan" => "BienBan",
                "KetLuan" => "ThongBaoKetLuan",
                "TongHop" => "BaoCaoTongHop",
                _ => "CuocHop"
            };
            
            var dialog = new SaveFileDialog
            {
                Title = "Lưu file Word",
                Filter = "Word Document (*.docx)|*.docx",
                FileName = $"{prefix}_{safeTitle}_{meeting.StartTime:yyyyMMdd}.docx",
                DefaultExt = ".docx"
            };
            
            if (dialog.ShowDialog() == true)
            {
                switch (exportType)
                {
                    case "BienBan":
                        _exportService.ExportBienBan(meeting, dialog.FileName);
                        break;
                    case "KetLuan":
                        _exportService.ExportKetLuan(meeting, dialog.FileName);
                        break;
                    case "TongHop":
                        _exportService.ExportBaoCaoTongHop(meeting, dialog.FileName);
                        break;
                }
                
                var openResult = MessageBox.Show(
                    $"✅ Đã xuất file thành công!\n\n📄 {dialog.FileName}\n\nBạn có muốn mở file ngay?",
                    "Xuất Word thành công",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information);
                
                if (openResult == MessageBoxResult.Yes)
                {
                    Process.Start(new ProcessStartInfo(dialog.FileName) { UseShellExecute = true });
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi xuất file Word:\n{ex.Message}", "Lỗi",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    // ======= MORE ACTIONS MENU =======
    
    private void ShowMoreMenu(Meeting meeting, Button anchor)
    {
        var menu = new ContextMenu();
        
        // Duplicate
        var itemDuplicate = new MenuItem { Header = "📋 Nhân bản cuộc họp" };
        itemDuplicate.Click += (s, _) => DuplicateMeeting(meeting);
        menu.Items.Add(itemDuplicate);
        
        // Lưu mẫu
        var itemSaveTemplate = new MenuItem { Header = "💾 Lưu làm mẫu cuộc họp" };
        itemSaveTemplate.Click += (s, _) => SaveMeetingAsTemplate(meeting);
        menu.Items.Add(itemSaveTemplate);
        
        menu.Items.Add(new Separator());
        
        // Status changes
        var statusMenu = new MenuItem { Header = "🔄 Đổi trạng thái" };
        foreach (MeetingStatus status in Enum.GetValues(typeof(MeetingStatus)))
        {
            if (status == meeting.Status) continue;
            var item = new MenuItem 
            { 
                Header = $"{MeetingHelper.GetStatusIcon(status)} {MeetingHelper.GetStatusName(status)}",
                Tag = status
            };
            var statusValue = status;
            item.Click += (s, _) => QuickChangeStatus(meeting.Id, statusValue);
            statusMenu.Items.Add(item);
        }
        menu.Items.Add(statusMenu);
        
        menu.Items.Add(new Separator());
        
        // Delete
        var itemDelete = new MenuItem { Header = "🗑️ Xóa cuộc họp", Foreground = new SolidColorBrush(Colors.Red) };
        itemDelete.Click += (s, _) =>
        {
            var result = MessageBox.Show(
                $"Bạn có chắc muốn xóa cuộc họp:\n\"{meeting.Title}\"?",
                "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                _meetingService.DeleteMeeting(meeting.Id);
                LoadMeetings();
                LoadStatistics();
            }
        };
        menu.Items.Add(itemDelete);
        
        menu.PlacementTarget = anchor;
        menu.IsOpen = true;
    }
    
    private void QuickChangeStatus(string meetingId, MeetingStatus newStatus)
    {
        try
        {
            var meeting = _meetingService.GetMeetingById(meetingId);
            if (meeting == null) return;
            
            meeting.Status = newStatus;
            meeting.ModifiedDate = DateTime.Now;
            meeting.ModifiedBy = Environment.UserName;
            _meetingService.UpdateMeeting(meeting);
            LoadMeetings();
            LoadStatistics();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi đổi trạng thái:\n{ex.Message}", "Lỗi",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    private void DuplicateMeeting(Meeting source)
    {
        try
        {
            var newMeeting = new Meeting
            {
                Title = $"[Bản sao] {source.Title}",
                MeetingNumber = "",
                Type = source.Type,
                Level = source.Level,
                Status = MeetingStatus.Scheduled,
                Priority = source.Priority,
                StartTime = DateTime.Today.AddDays(7).AddHours(source.StartTime.Hour).AddMinutes(source.StartTime.Minute),
                EndTime = source.EndTime.HasValue ? DateTime.Today.AddDays(7).AddHours(source.EndTime.Value.Hour).AddMinutes(source.EndTime.Value.Minute) : null,
                IsAllDay = source.IsAllDay,
                Location = source.Location,
                Format = source.Format,
                OnlineLink = source.OnlineLink,
                ChairPerson = source.ChairPerson,
                ChairPersonTitle = source.ChairPersonTitle,
                Secretary = source.Secretary,
                OrganizingUnit = source.OrganizingUnit,
                Attendees = source.Attendees?.Select(a => new MeetingAttendee
                {
                    Name = a.Name, Position = a.Position, Unit = a.Unit,
                    Phone = a.Phone, Role = a.Role, AttendanceStatus = AttendanceStatus.Invited
                }).ToList() ?? new(),
                Agenda = source.Agenda,
                Tags = source.Tags?.ToArray() ?? Array.Empty<string>(),
                Documents = new List<MeetingDocument>() // Tài liệu mới cho cuộc họp mới
            };
            
            // Mở dialog edit để user chỉnh sửa trước khi lưu
            var dialog = new MeetingEditDialog(newMeeting, _meetingService, _documentService)
            {
                Owner = Window.GetWindow(this),
                Title = "Nhân bản cuộc họp"
            };
            
            if (dialog.ShowDialog() == true)
            {
                LoadMeetings();
                LoadStatistics();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi nhân bản cuộc họp:\n{ex.Message}", "Lỗi",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Lưu cuộc họp hiện tại thành mẫu cuộc họp
    /// </summary>
    private void SaveMeetingAsTemplate(Meeting meeting)
    {
        try
        {
            // Hỏi tên mẫu bằng simple dialog
            var defaultName = meeting.Title.Replace("[Bản sao] ", "");
            
            var inputWindow = new Window
            {
                Title = "Lưu mẫu cuộc họp",
                Width = 450, Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Window.GetWindow(this),
                ResizeMode = ResizeMode.NoResize
            };
            var inputStack = new StackPanel { Margin = new Thickness(20) };
            inputStack.Children.Add(new TextBlock
            {
                Text = $"Nhập tên mẫu cuộc họp:\n(Dựa trên: {meeting.Title})",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 12)
            });
            var inputBox = new TextBox { Text = defaultName, FontSize = 14 };
            inputStack.Children.Add(inputBox);
            var okBtn = new Button
            {
                Content = "Lưu mẫu",
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 12, 0, 0),
                Padding = new Thickness(20, 6, 20, 6),
                Style = (Style)FindResource("MaterialDesignRaisedButton")
            };
            okBtn.Click += (s2, e2) => { inputWindow.DialogResult = true; };
            inputStack.Children.Add(okBtn);
            inputWindow.Content = inputStack;

            if (inputWindow.ShowDialog() != true || string.IsNullOrWhiteSpace(inputBox.Text)) return;
            var input = inputBox.Text;

            var template = _meetingService.SaveAsTemplate(meeting, input.Trim());
            MessageBox.Show(
                $"✅ Đã lưu mẫu cuộc họp:\n\n" +
                $"📋 {template.TemplateName}\n" +
                $"📂 Loại: {MeetingHelper.GetTypeName(template.Type)}\n" +
                $"📍 Địa điểm: {template.Location}\n\n" +
                "Bạn có thể tạo cuộc họp mới từ mẫu bằng nút \"Từ mẫu\" ở thanh công cụ.",
                "Đã lưu mẫu",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi lưu mẫu:\n{ex.Message}", "Lỗi",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Tạo cuộc họp từ mẫu đã lưu — hiện danh sách mẫu cho user chọn
    /// </summary>
    private void CreateFromTemplate_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var templates = _meetingService.GetMeetingTemplates();
            if (templates.Count == 0)
            {
                MessageBox.Show(
                    "Chưa có mẫu cuộc họp nào!\n\n" +
                    "Để lưu mẫu: nhấn nút ⋮ (thêm tùy chọn) trên bất kỳ cuộc họp nào → \"Lưu làm mẫu\".",
                    "Chưa có mẫu",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            var menu = new ContextMenu();
            foreach (var tmpl in templates)
            {
                var typeName = MeetingHelper.GetTypeName(tmpl.Type);
                var item = new MenuItem
                {
                    Header = $"📋 {tmpl.TemplateName}  ({typeName})",
                    ToolTip = $"Địa điểm: {tmpl.Location}\nChủ trì: {tmpl.ChairPerson}\n{tmpl.Attendees?.Count ?? 0} thành viên",
                    Tag = tmpl.Id
                };
                item.Click += (s, _) =>
                {
                    var meeting = _meetingService.CreateFromTemplate(tmpl, DateTime.Today);
                    // Mở dialog đầy đủ để user chỉnh sửa trước khi lưu
                    var dialog = new MeetingEditDialog(meeting, _meetingService, _documentService)
                    {
                        Owner = Window.GetWindow(this)
                    };
                    if (dialog.ShowDialog() == true)
                    {
                        LoadMeetings();
                        LoadStatistics();
                    }
                };
                menu.Items.Add(item);
            }

            menu.Items.Add(new Separator());
            var deleteMenu = new MenuItem { Header = "🗑️ Quản lý mẫu..." };
            foreach (var tmpl in templates)
            {
                var delItem = new MenuItem
                {
                    Header = $"Xóa: {tmpl.TemplateName}",
                    Foreground = new SolidColorBrush(Colors.Red),
                    Tag = tmpl.Id
                };
                var tmplId = tmpl.Id;
                var tmplName = tmpl.TemplateName;
                delItem.Click += (s, _) =>
                {
                    if (MessageBox.Show($"Xóa mẫu \"{tmplName}\"?", "Xác nhận",
                        MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        _meetingService.DeleteMeetingTemplate(tmplId);
                        MessageBox.Show("Đã xóa mẫu.", "Thông báo");
                    }
                };
                deleteMenu.Items.Add(delItem);
            }
            menu.Items.Add(deleteMenu);

            if (sender is Button btn)
            {
                menu.PlacementTarget = btn;
                menu.IsOpen = true;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi tạo từ mẫu:\n{ex.Message}", "Lỗi",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    // ======= EXPORT ALL (header button) =======
    
    private void ExportAllMeetings_Click(object sender, RoutedEventArgs e)
    {
        if (_currentMeetings.Count == 0)
        {
            MessageBox.Show("Không có cuộc họp nào để xuất!", "Thông báo",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        
        var menu = new ContextMenu();
        
        var itemAll = new MenuItem { Header = $"📋 Xuất toàn bộ {_currentMeetings.Count} cuộc họp (Báo cáo tổng hợp)" };
        itemAll.Click += (s, _) => ExportMultipleMeetings(_currentMeetings);
        menu.Items.Add(itemAll);
        
        if (_currentMeetings.Count > 1)
        {
            var completed = _currentMeetings.Where(m => m.Status == MeetingStatus.Completed).ToList();
            if (completed.Count > 0 && completed.Count != _currentMeetings.Count)
            {
                var itemCompleted = new MenuItem { Header = $"✅ Chỉ xuất {completed.Count} cuộc họp đã hoàn thành" };
                itemCompleted.Click += (s, _) => ExportMultipleMeetings(completed);
                menu.Items.Add(itemCompleted);
            }
            
            var scheduled = _currentMeetings.Where(m => m.Status == MeetingStatus.Scheduled).ToList();
            if (scheduled.Count > 0 && scheduled.Count != _currentMeetings.Count)
            {
                var itemScheduled = new MenuItem { Header = $"📅 Chỉ xuất {scheduled.Count} cuộc họp sắp tới" };
                itemScheduled.Click += (s, _) => ExportMultipleMeetings(scheduled);
                menu.Items.Add(itemScheduled);
            }
        }
        
        menu.PlacementTarget = (Button)sender;
        menu.IsOpen = true;
    }
    
    private void ExportMultipleMeetings(List<Meeting> meetings)
    {
        try
        {
            var dialog = new SaveFileDialog
            {
                Title = "Lưu báo cáo tổng hợp các cuộc họp",
                Filter = "Word Document (*.docx)|*.docx",
                FileName = $"TongHop_CuocHop_{DateTime.Now:yyyyMMdd}.docx",
                DefaultExt = ".docx"
            };
            
            if (dialog.ShowDialog() == true)
            {
                ExportMultipleMeetingsToWord(meetings, dialog.FileName);
                
                var openResult = MessageBox.Show(
                    $"✅ Đã xuất {meetings.Count} cuộc họp thành công!\n\n📄 {dialog.FileName}\n\nBạn có muốn mở file ngay?",
                    "Xuất Word thành công",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information);
                
                if (openResult == MessageBoxResult.Yes)
                {
                    Process.Start(new ProcessStartInfo(dialog.FileName) { UseShellExecute = true });
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi xuất file Word:\n{ex.Message}", "Lỗi",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    private void ExportMultipleMeetingsToWord(List<Meeting> meetings, string outputPath)
    {
        _exportService.ExportTongHopNhieuCuocHop(meetings, outputPath);
    }
    
    #endregion
}
