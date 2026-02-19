using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AIVanBan.Core.Models;
using AIVanBan.Core.Services;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;

namespace AIVanBan.Desktop.Views;

public partial class MeetingEditDialog : Window
{
    private readonly MeetingService _meetingService;
    private readonly DocumentService? _documentService;
    private readonly SimpleAlbumService _albumService;
    private Meeting _meeting;
    private bool _isNew;
    
    // Danh sách attendees, tasks, documents (working copies)
    private List<MeetingAttendee> _attendees = new();
    private List<MeetingTask> _tasks = new();
    private List<MeetingDocument> _documents = new();
    private List<string> _attachmentPaths = new();
    private List<string> _relatedAlbumIds = new();
    
    public MeetingEditDialog(Meeting? meeting, MeetingService meetingService, DocumentService? documentService = null)
    {
        InitializeComponent();
        _meetingService = meetingService;
        _documentService = documentService;
        _albumService = new SimpleAlbumService();
        _isNew = meeting == null;
        _meeting = meeting ?? new Meeting();
        
        InitializeComboBoxes();
        
        if (_isNew)
        {
            txtDialogTitle.Text = "THÊM CUỘC HỌP MỚI";
            Title = "Thêm cuộc họp mới";
            // Default values
            dpStartDate.SelectedDate = DateTime.Today;
            tpStartTime.SelectedTime = DateTime.Today.AddHours(8);
        }
        else
        {
            txtDialogTitle.Text = "CHỈNH SỬA CUỘC HỌP";
            Title = $"Sửa: {_meeting.Title}";
            LoadMeetingData();
        }
    }
    
    #region Initialize
    
    private void InitializeComboBoxes()
    {
        // Meeting Type
        foreach (MeetingType type in Enum.GetValues(typeof(MeetingType)))
        {
            var item = new ComboBoxItem
            {
                Content = MeetingHelper.GetTypeName(type),
                Tag = type
            };
            cboMeetingType.Items.Add(item);
        }
        cboMeetingType.SelectedIndex = Array.IndexOf(Enum.GetValues(typeof(MeetingType)), MeetingType.HopCoQuan);
        
        // Meeting Level
        foreach (MeetingLevel level in Enum.GetValues(typeof(MeetingLevel)))
        {
            var item = new ComboBoxItem
            {
                Content = MeetingHelper.GetLevelName(level),
                Tag = level
            };
            cboLevel.Items.Add(item);
        }
        cboLevel.SelectedIndex = 0;
        
        // Status
        foreach (MeetingStatus status in Enum.GetValues(typeof(MeetingStatus)))
        {
            var item = new ComboBoxItem
            {
                Content = $"{MeetingHelper.GetStatusIcon(status)} {MeetingHelper.GetStatusName(status)}",
                Tag = status
            };
            cboStatus.Items.Add(item);
        }
        cboStatus.SelectedIndex = 0;
        
        // Format
        foreach (MeetingFormat format in Enum.GetValues(typeof(MeetingFormat)))
        {
            var item = new ComboBoxItem
            {
                Content = $"{MeetingHelper.GetFormatIcon(format)} {MeetingHelper.GetFormatName(format)}",
                Tag = format
            };
            cboFormat.Items.Add(item);
        }
        cboFormat.SelectedIndex = 0;
    }
    
    private void LoadMeetingData()
    {
        // Tab 1: Thông tin chung
        txtTitle.Text = _meeting.Title;
        txtMeetingNumber.Text = _meeting.MeetingNumber;
        
        // Select correct combo items
        SelectComboByTag(cboMeetingType, _meeting.Type);
        SelectComboByTag(cboLevel, _meeting.Level);
        SelectComboByTag(cboStatus, _meeting.Status);
        SelectComboByTag(cboFormat, _meeting.Format);
        SelectComboByTag(cboPriority, _meeting.Priority.ToString());
        
        dpStartDate.SelectedDate = _meeting.StartTime.Date;
        tpStartTime.SelectedTime = _meeting.StartTime;
        
        if (_meeting.EndTime.HasValue)
        {
            dpEndDate.SelectedDate = _meeting.EndTime.Value.Date;
            tpEndTime.SelectedTime = _meeting.EndTime.Value;
        }
        
        chkAllDay.IsChecked = _meeting.IsAllDay;
        txtLocation.Text = _meeting.Location;
        txtOnlineLink.Text = _meeting.OnlineLink;
        
        // Show online link if format is not TrucTiep
        if (_meeting.Format != MeetingFormat.TrucTiep)
            txtOnlineLink.Visibility = Visibility.Visible;
        
        txtTags.Text = _meeting.Tags != null ? string.Join(", ", _meeting.Tags) : "";
        
        // Tab 2: Thành phần
        txtOrganizingUnit.Text = _meeting.OrganizingUnit;
        txtChairPerson.Text = _meeting.ChairPerson;
        txtChairPersonTitle.Text = _meeting.ChairPersonTitle;
        txtSecretary.Text = _meeting.Secretary;
        
        _attendees = _meeting.Attendees?.ToList() ?? new List<MeetingAttendee>();
        RefreshAttendeeList();
        
        // Tab 3: Nội dung
        txtAgenda.Text = _meeting.Agenda;
        txtContent.Text = _meeting.Content;
        txtConclusion.Text = _meeting.Conclusion;
        txtPersonalNotes.Text = _meeting.PersonalNotes;
        
        // Tab 4: Nhiệm vụ
        _tasks = _meeting.Tasks?.ToList() ?? new List<MeetingTask>();
        RefreshTaskList();
        
        // Tab 5: Văn bản cuộc họp
        _documents = _meeting.Documents?.ToList() ?? new List<MeetingDocument>();
        
        // Migrate legacy fields if no documents exist
        if (_documents.Count == 0)
        {
            if (!string.IsNullOrWhiteSpace(_meeting.InvitationDocId))
                _documents.Add(new MeetingDocument { DocumentType = MeetingDocumentType.GiayMoi, DocumentNumber = _meeting.InvitationDocId, Title = "Giấy mời họp" });
            if (!string.IsNullOrWhiteSpace(_meeting.MinutesDocId))
                _documents.Add(new MeetingDocument { DocumentType = MeetingDocumentType.BienBan, DocumentNumber = _meeting.MinutesDocId, Title = "Biên bản cuộc họp" });
            if (!string.IsNullOrWhiteSpace(_meeting.ConclusionDocId))
                _documents.Add(new MeetingDocument { DocumentType = MeetingDocumentType.ThongBaoKetLuan, DocumentNumber = _meeting.ConclusionDocId, Title = "Thông báo kết luận" });
        }
        RefreshDocumentList();
        
        _attachmentPaths = _meeting.AttachmentPaths?.ToList() ?? new List<string>();
        RefreshAttachmentList();
        
        // Tab 6: Album ảnh liên quan
        _relatedAlbumIds = _meeting.RelatedAlbumIds?.ToList() ?? new List<string>();
        RefreshLinkedAlbumList();
        
        // Created info
        txtCreatedInfo.Text = $"Tạo bởi: {_meeting.CreatedBy} lúc {_meeting.CreatedDate:dd/MM/yyyy HH:mm}";
        if (_meeting.ModifiedDate.HasValue)
            txtCreatedInfo.Text += $" | Sửa lần cuối: {_meeting.ModifiedDate:dd/MM/yyyy HH:mm}";
    }
    
    private void SelectComboByTag(ComboBox combo, object tagValue)
    {
        for (int i = 0; i < combo.Items.Count; i++)
        {
            if (combo.Items[i] is ComboBoxItem item && item.Tag?.ToString() == tagValue.ToString())
            {
                combo.SelectedIndex = i;
                return;
            }
        }
    }
    
    #endregion
    
    #region Attendee Management
    
    private void AddAttendee_Click(object sender, RoutedEventArgs e)
    {
        _attendees.Add(new MeetingAttendee
        {
            AttendanceStatus = AttendanceStatus.Invited
        });
        RefreshAttendeeList();
    }
    
    private void RefreshAttendeeList()
    {
        attendeeListPanel.Children.Clear();
        txtNoAttendees.Visibility = _attendees.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        
        for (int i = 0; i < _attendees.Count; i++)
        {
            var attendee = _attendees[i];
            var index = i;
            
            var card = new Border
            {
                BorderBrush = new SolidColorBrush(Color.FromRgb(0xE0, 0xE0, 0xE0)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(12, 8, 12, 8),
                Margin = new Thickness(0, 0, 0, 6),
                Background = Brushes.White
            };
            
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(130) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(36) });
            
            grid.RowDefinitions.Add(new RowDefinition());
            
            // Name
            var txtName = new TextBox
            {
                Text = attendee.Name,
                Tag = index,
                Margin = new Thickness(0, 0, 6, 0)
            };
            HintAssist.SetHint(txtName, "Họ tên");
            txtName.TextChanged += (s, ev) => { if (index < _attendees.Count) _attendees[index].Name = txtName.Text; };
            Grid.SetColumn(txtName, 0);
            grid.Children.Add(txtName);
            
            // Position
            var txtPos = new TextBox
            {
                Text = attendee.Position,
                Margin = new Thickness(0, 0, 6, 0)
            };
            HintAssist.SetHint(txtPos, "Chức vụ");
            txtPos.TextChanged += (s, ev) => { if (index < _attendees.Count) _attendees[index].Position = txtPos.Text; };
            Grid.SetColumn(txtPos, 1);
            grid.Children.Add(txtPos);
            
            // Unit
            var txtUnit = new TextBox
            {
                Text = attendee.Unit,
                Margin = new Thickness(0, 0, 6, 0)
            };
            HintAssist.SetHint(txtUnit, "Đơn vị");
            txtUnit.TextChanged += (s, ev) => { if (index < _attendees.Count) _attendees[index].Unit = txtUnit.Text; };
            Grid.SetColumn(txtUnit, 2);
            grid.Children.Add(txtUnit);
            
            // Attendance Status
            var cboAttendance = new ComboBox
            {
                Margin = new Thickness(0, 0, 6, 0),
                FontSize = 11
            };
            foreach (AttendanceStatus status in Enum.GetValues(typeof(AttendanceStatus)))
            {
                cboAttendance.Items.Add(new ComboBoxItem
                {
                    Content = MeetingHelper.GetAttendanceStatusName(status),
                    Tag = status
                });
            }
            SelectComboByTag(cboAttendance, attendee.AttendanceStatus);
            cboAttendance.SelectionChanged += (s, ev) =>
            {
                if (index < _attendees.Count && cboAttendance.SelectedItem is ComboBoxItem selItem && selItem.Tag is AttendanceStatus st)
                    _attendees[index].AttendanceStatus = st;
            };
            Grid.SetColumn(cboAttendance, 3);
            grid.Children.Add(cboAttendance);
            
            // Delete button
            var btnDel = new Button
            {
                Style = (Style)FindResource("MaterialDesignIconButton"),
                Width = 30, Height = 30,
                Tag = index,
                ToolTip = "Xóa"
            };
            btnDel.Content = new PackIcon { Kind = PackIconKind.Close, Width = 16, Height = 16 };
            btnDel.Click += (s, ev) =>
            {
                if (index < _attendees.Count)
                {
                    _attendees.RemoveAt(index);
                    RefreshAttendeeList();
                }
            };
            Grid.SetColumn(btnDel, 4);
            grid.Children.Add(btnDel);
            
            card.Child = grid;
            attendeeListPanel.Children.Add(card);
        }
    }
    
    #endregion
    
    #region Task Management
    
    private void AddTask_Click(object sender, RoutedEventArgs e)
    {
        _tasks.Add(new MeetingTask());
        RefreshTaskList();
    }
    
    private void RefreshTaskList()
    {
        taskListPanel.Children.Clear();
        
        if (_tasks.Count > 0)
        {
            taskStatsBar.Visibility = Visibility.Visible;
            var completed = _tasks.Count(t => t.TaskStatus == MeetingTaskStatus.Completed);
            var inProgress = _tasks.Count(t => t.TaskStatus == MeetingTaskStatus.InProgress);
            var overdue = _tasks.Count(t => t.TaskStatus == MeetingTaskStatus.Overdue ||
                ((t.TaskStatus == MeetingTaskStatus.NotStarted || t.TaskStatus == MeetingTaskStatus.InProgress) &&
                 t.Deadline.HasValue && t.Deadline.Value < DateTime.Now));
            
            txtTaskTotal.Text = $"Tổng: {_tasks.Count}";
            txtTaskCompleted.Text = $"✅ Hoàn thành: {completed}";
            txtTaskInProgress.Text = $"🔄 Đang làm: {inProgress}";
            txtTaskOverdue.Text = $"🔴 Quá hạn: {overdue}";
            txtTaskStats.Text = $"({completed}/{_tasks.Count} hoàn thành)";
        }
        else
        {
            taskStatsBar.Visibility = Visibility.Collapsed;
            txtTaskStats.Text = "";
            
            var emptyText = new TextBlock
            {
                Text = "Chưa có nhiệm vụ nào. Nhấn 'Thêm nhiệm vụ' để bắt đầu theo dõi.",
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(0x9E, 0x9E, 0x9E)),
                FontStyle = FontStyles.Italic,
                Margin = new Thickness(0, 8, 0, 0)
            };
            taskListPanel.Children.Add(emptyText);
            return;
        }
        
        for (int i = 0; i < _tasks.Count; i++)
        {
            var task = _tasks[i];
            var index = i;
            
            // Determine card color based on status
            var isOverdue = (task.TaskStatus == MeetingTaskStatus.NotStarted || task.TaskStatus == MeetingTaskStatus.InProgress) &&
                           task.Deadline.HasValue && task.Deadline.Value < DateTime.Now;
            
            var borderColor = isOverdue || task.TaskStatus == MeetingTaskStatus.Overdue ? "#FFCDD2" :
                             task.TaskStatus == MeetingTaskStatus.Completed ? "#C8E6C9" :
                             task.TaskStatus == MeetingTaskStatus.InProgress ? "#BBDEFB" : "#E0E0E0";
            
            var card = new Border
            {
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(borderColor)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(14, 10, 14, 10),
                Margin = new Thickness(0, 0, 0, 8),
                Background = Brushes.White
            };
            
            var mainPanel = new StackPanel();
            
            // Row 1: Task title + delete button
            var row1 = new DockPanel { Margin = new Thickness(0, 0, 0, 6) };
            
            var txtTaskTitle = new TextBox
            {
                Text = task.Title,
                FontSize = 13,
                FontWeight = FontWeights.SemiBold
            };
            HintAssist.SetHint(txtTaskTitle, "Nội dung nhiệm vụ *");
            txtTaskTitle.TextChanged += (s, ev) => { if (index < _tasks.Count) _tasks[index].Title = txtTaskTitle.Text; };
            row1.Children.Add(txtTaskTitle);
            
            var btnDelTask = new Button
            {
                Style = (Style)FindResource("MaterialDesignIconButton"),
                Width = 30, Height = 30,
                ToolTip = "Xóa nhiệm vụ",
                HorizontalAlignment = HorizontalAlignment.Right
            };
            DockPanel.SetDock(btnDelTask, Dock.Right);
            btnDelTask.Content = new PackIcon { Kind = PackIconKind.Delete, Width = 16, Height = 16,
                Foreground = new SolidColorBrush(Color.FromRgb(0xE5, 0x39, 0x35)) };
            btnDelTask.Click += (s, ev) =>
            {
                if (index < _tasks.Count)
                {
                    _tasks.RemoveAt(index);
                    RefreshTaskList();
                }
            };
            DockPanel.SetDock(btnDelTask, Dock.Right);
            row1.Children.Add(btnDelTask);
            mainPanel.Children.Add(row1);
            
            // Row 2: Assignee + Unit + Deadline + Status
            var row2 = new Grid { Margin = new Thickness(0, 0, 0, 4) };
            row2.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            row2.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(6) });
            row2.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            row2.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(6) });
            row2.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(140) });
            row2.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(6) });
            row2.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(155) });
            
            // Assignee
            var txtAssignee = new TextBox { Text = task.AssignedTo, FontSize = 12 };
            HintAssist.SetHint(txtAssignee, "Người thực hiện");
            txtAssignee.TextChanged += (s, ev) => { if (index < _tasks.Count) _tasks[index].AssignedTo = txtAssignee.Text; };
            Grid.SetColumn(txtAssignee, 0);
            row2.Children.Add(txtAssignee);
            
            // Unit
            var txtTaskUnit = new TextBox { Text = task.AssignedUnit, FontSize = 12 };
            HintAssist.SetHint(txtTaskUnit, "Đơn vị");
            txtTaskUnit.TextChanged += (s, ev) => { if (index < _tasks.Count) _tasks[index].AssignedUnit = txtTaskUnit.Text; };
            Grid.SetColumn(txtTaskUnit, 2);
            row2.Children.Add(txtTaskUnit);
            
            // Deadline
            var dpDeadline = new DatePicker
            {
                SelectedDate = task.Deadline,
                FontSize = 11
            };
            HintAssist.SetHint(dpDeadline, "Hạn");
            dpDeadline.SelectedDateChanged += (s, ev) => 
            { 
                if (index < _tasks.Count) _tasks[index].Deadline = dpDeadline.SelectedDate; 
            };
            Grid.SetColumn(dpDeadline, 4);
            row2.Children.Add(dpDeadline);
            
            // Task Status
            var cboTaskStatus = new ComboBox { FontSize = 11 };
            foreach (MeetingTaskStatus ts in Enum.GetValues(typeof(MeetingTaskStatus)))
            {
                cboTaskStatus.Items.Add(new ComboBoxItem
                {
                    Content = MeetingHelper.GetTaskStatusName(ts),
                    Tag = ts
                });
            }
            SelectComboByTag(cboTaskStatus, task.TaskStatus);
            cboTaskStatus.SelectionChanged += (s, ev) =>
            {
                if (index < _tasks.Count && cboTaskStatus.SelectedItem is ComboBoxItem selItem && selItem.Tag is MeetingTaskStatus ts)
                {
                    _tasks[index].TaskStatus = ts;
                    if (ts == MeetingTaskStatus.Completed)
                        _tasks[index].CompletionDate = DateTime.Now;
                    RefreshTaskList(); // Refresh to update colors
                }
            };
            Grid.SetColumn(cboTaskStatus, 6);
            row2.Children.Add(cboTaskStatus);
            
            mainPanel.Children.Add(row2);
            
            // Row 3: Notes
            var txtTaskNotes = new TextBox
            {
                Text = task.Notes,
                FontSize = 11,
                Margin = new Thickness(0, 4, 0, 0)
            };
            HintAssist.SetHint(txtTaskNotes, "Ghi chú / Kết quả thực hiện");
            txtTaskNotes.TextChanged += (s, ev) => { if (index < _tasks.Count) _tasks[index].Notes = txtTaskNotes.Text; };
            mainPanel.Children.Add(txtTaskNotes);
            
            card.Child = mainPanel;
            taskListPanel.Children.Add(card);
        }
    }
    
    #endregion
    
    #region Document Management
    
    private void AddDocument_Click(object sender, RoutedEventArgs e)
    {
        var contextMenu = new ContextMenu();
        
        // === Nhóm 1: Thêm từng loại văn bản ===
        var addHeader = new MenuItem
        {
            Header = "📝 THÊM VĂN BẢN MỚI",
            IsEnabled = false,
            FontWeight = FontWeights.Bold,
            FontSize = 12
        };
        contextMenu.Items.Add(addHeader);
        
        foreach (MeetingDocumentType docType in Enum.GetValues(typeof(MeetingDocumentType)))
        {
            var menuItem = new MenuItem
            {
                Header = $"  {MeetingHelper.GetDocumentTypeIcon(docType)} {MeetingHelper.GetDocumentTypeName(docType)}",
                Tag = docType,
                FontSize = 13
            };
            menuItem.Click += (s, ev) =>
            {
                var type = (MeetingDocumentType)((MenuItem)s!).Tag;
                _documents.Add(new MeetingDocument { DocumentType = type });
                RefreshDocumentList();
            };
            contextMenu.Items.Add(menuItem);
        }
        
        // === Nhóm 2: Liên kết từ module Quản lý văn bản ===
        if (_documentService != null)
        {
            contextMenu.Items.Add(new Separator());
            
            var linkHeader = new MenuItem
            {
                Header = "🔗 LIÊN KẾT TỪ QUẢN LÝ VĂN BẢN",
                IsEnabled = false,
                FontWeight = FontWeights.Bold,
                FontSize = 12
            };
            contextMenu.Items.Add(linkHeader);
            
            var linkItem = new MenuItem
            {
                Header = "  🔍 Tìm & liên kết văn bản đã có trong hệ thống...",
                FontSize = 13
            };
            linkItem.Click += (s, ev) => ShowDocumentPickerDialog();
            contextMenu.Items.Add(linkItem);
        }
        
        // === Nhóm 3: Thêm nhanh bộ chuẩn ===
        contextMenu.Items.Add(new Separator());
        
        var quickHeader = new MenuItem
        {
            Header = "⚡ THÊM NHANH",
            IsEnabled = false,
            FontWeight = FontWeights.Bold,
            FontSize = 12
        };
        contextMenu.Items.Add(quickHeader);
        
        var quickStandard = new MenuItem
        {
            Header = "  📋 Bộ văn bản chuẩn (Giấy mời + Chương trình + Tài liệu)",
            FontSize = 13
        };
        quickStandard.Click += (s, ev) =>
        {
            if (!_documents.Any(d => d.DocumentType == MeetingDocumentType.GiayMoi))
                _documents.Add(new MeetingDocument { DocumentType = MeetingDocumentType.GiayMoi });
            if (!_documents.Any(d => d.DocumentType == MeetingDocumentType.ChuongTrinh))
                _documents.Add(new MeetingDocument { DocumentType = MeetingDocumentType.ChuongTrinh });
            if (!_documents.Any(d => d.DocumentType == MeetingDocumentType.TaiLieuHop))
                _documents.Add(new MeetingDocument { DocumentType = MeetingDocumentType.TaiLieuHop });
            RefreshDocumentList();
        };
        contextMenu.Items.Add(quickStandard);
        
        var quickPostMeeting = new MenuItem
        {
            Header = "  📌 Bộ văn bản sau họp (Biên bản + Kết luận)",
            FontSize = 13
        };
        quickPostMeeting.Click += (s, ev) =>
        {
            if (!_documents.Any(d => d.DocumentType == MeetingDocumentType.BienBan))
                _documents.Add(new MeetingDocument { DocumentType = MeetingDocumentType.BienBan });
            if (!_documents.Any(d => d.DocumentType == MeetingDocumentType.ThongBaoKetLuan))
                _documents.Add(new MeetingDocument { DocumentType = MeetingDocumentType.ThongBaoKetLuan });
            RefreshDocumentList();
        };
        contextMenu.Items.Add(quickPostMeeting);
        
        contextMenu.IsOpen = true;
    }
    
    /// <summary>
    /// Dialog tìm kiếm và liên kết văn bản từ module Quản lý văn bản
    /// </summary>
    private void ShowDocumentPickerDialog()
    {
        if (_documentService == null) return;
        
        var dialog = new Window
        {
            Title = "🔍 Liên kết văn bản từ hệ thống",
            Width = 750,
            Height = 520,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = this,
            ResizeMode = ResizeMode.CanResizeWithGrip
        };
        
        var mainPanel = new Grid();
        mainPanel.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        mainPanel.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        mainPanel.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        
        // === Search bar ===
        var searchPanel = new DockPanel { Margin = new Thickness(16, 12, 16, 8) };
        var txtSearch = new TextBox
        {
            FontSize = 14,
            Padding = new Thickness(8, 6, 8, 6)
        };
        HintAssist.SetHint(txtSearch, "🔍 Tìm theo số văn bản, trích yếu, cơ quan ban hành...");
        searchPanel.Children.Add(txtSearch);
        Grid.SetRow(searchPanel, 0);
        mainPanel.Children.Add(searchPanel);
        
        // === Document list ===
        var listPanel = new StackPanel();
        var scrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Margin = new Thickness(16, 0, 16, 0),
            Content = listPanel
        };
        Grid.SetRow(scrollViewer, 1);
        mainPanel.Children.Add(scrollViewer);
        
        // === Status bar ===
        var statusText = new TextBlock
        {
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.FromRgb(0x75, 0x75, 0x75)),
            Margin = new Thickness(16, 8, 16, 12)
        };
        Grid.SetRow(statusText, 2);
        mainPanel.Children.Add(statusText);
        
        // === Load documents ===
        void LoadDocuments(string keyword = "")
        {
            listPanel.Children.Clear();
            
            List<Document> docs;
            if (string.IsNullOrWhiteSpace(keyword))
                docs = _documentService.GetAllDocuments().OrderByDescending(d => d.IssueDate).Take(50).ToList();
            else
                docs = _documentService.SearchDocuments(keyword).Take(50).ToList();
            
            if (docs.Count == 0)
            {
                listPanel.Children.Add(new TextBlock
                {
                    Text = "Không tìm thấy văn bản nào.",
                    FontSize = 13,
                    Foreground = new SolidColorBrush(Color.FromRgb(0x9E, 0x9E, 0x9E)),
                    FontStyle = FontStyles.Italic,
                    Margin = new Thickness(0, 20, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Center
                });
                statusText.Text = "0 kết quả";
                return;
            }
            
            statusText.Text = $"{docs.Count} văn bản" + (docs.Count == 50 ? " (hiện 50 đầu tiên)" : "");
            
            // Check which docs are already linked
            var alreadyLinked = _documents
                .Where(d => !string.IsNullOrEmpty(d.LinkedDocumentId))
                .Select(d => d.LinkedDocumentId)
                .ToHashSet();
            
            foreach (var doc in docs)
            {
                var isLinked = alreadyLinked.Contains(doc.Id);
                
                var card = new Border
                {
                    BorderBrush = new SolidColorBrush(isLinked ? Color.FromRgb(0x43, 0xA0, 0x47) : Color.FromRgb(0xE0, 0xE0, 0xE0)),
                    BorderThickness = new Thickness(1),
                    Background = isLinked ? new SolidColorBrush(Color.FromRgb(0xE8, 0xF5, 0xE9)) : Brushes.White,
                    Padding = new Thickness(12, 8, 12, 8),
                    Margin = new Thickness(0, 0, 0, 4),
                    CornerRadius = new CornerRadius(6),
                    Cursor = isLinked ? System.Windows.Input.Cursors.Arrow : System.Windows.Input.Cursors.Hand
                };
                
                var cardPanel = new DockPanel();
                
                // Right: Link button or linked badge
                if (isLinked)
                {
                    var badge = new TextBlock
                    {
                        Text = "✅ Đã liên kết",
                        FontSize = 11,
                        Foreground = new SolidColorBrush(Color.FromRgb(0x43, 0xA0, 0x47)),
                        FontWeight = FontWeights.SemiBold,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    DockPanel.SetDock(badge, Dock.Right);
                    cardPanel.Children.Add(badge);
                }
                else
                {
                    var btnLink = new Button
                    {
                        Style = (Style)FindResource("MaterialDesignOutlinedButton"),
                        Content = "🔗 Liên kết",
                        FontSize = 11,
                        Padding = new Thickness(8, 2, 8, 2),
                        Height = 28,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    var capturedDoc = doc;
                    btnLink.Click += (s, ev) =>
                    {
                        LinkDocumentToMeeting(capturedDoc);
                        dialog.Close();
                    };
                    DockPanel.SetDock(btnLink, Dock.Right);
                    cardPanel.Children.Add(btnLink);
                }
                
                // Left: Info
                var infoPanel = new StackPanel { Margin = new Thickness(0, 0, 12, 0) };
                
                // Title line
                var titleLine = new StackPanel { Orientation = Orientation.Horizontal };
                
                var typeIcon = doc.Type switch
                {
                    DocumentType.QuyetDinh => "📋",
                    DocumentType.CongVan => "📨",
                    DocumentType.BaoCao => "📊",
                    DocumentType.KeHoach => "📅",
                    DocumentType.ThongBao => "📌",
                    DocumentType.NghiQuyet => "📜",
                    DocumentType.ToTrinh => "📄",
                    _ => "📎"
                };
                
                titleLine.Children.Add(new TextBlock
                {
                    Text = typeIcon,
                    FontSize = 14,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 6, 0)
                });
                
                if (!string.IsNullOrEmpty(doc.Number))
                {
                    titleLine.Children.Add(new TextBlock
                    {
                        Text = doc.Number,
                        FontSize = 13,
                        FontWeight = FontWeights.Bold,
                        Foreground = new SolidColorBrush(Color.FromRgb(0x1B, 0x5E, 0x20)),
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(0, 0, 8, 0)
                    });
                }
                
                titleLine.Children.Add(new TextBlock
                {
                    Text = doc.Title,
                    FontSize = 13,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    MaxWidth = 400,
                    VerticalAlignment = VerticalAlignment.Center
                });
                infoPanel.Children.Add(titleLine);
                
                // Detail line
                var detailText = $"{doc.IssueDate:dd/MM/yyyy}";
                if (!string.IsNullOrEmpty(doc.Issuer))
                    detailText += $"  •  {doc.Issuer}";
                if (!string.IsNullOrEmpty(doc.Subject))
                    detailText += $"  •  {doc.Subject}";
                
                infoPanel.Children.Add(new TextBlock
                {
                    Text = detailText,
                    FontSize = 11,
                    Foreground = new SolidColorBrush(Color.FromRgb(0x75, 0x75, 0x75)),
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    Margin = new Thickness(20, 2, 0, 0)
                });
                
                cardPanel.Children.Add(infoPanel);
                card.Child = cardPanel;
                listPanel.Children.Add(card);
            }
        }
        
        // Initial load
        LoadDocuments();
        
        // Search on type
        System.Windows.Threading.DispatcherTimer? searchTimer = null;
        txtSearch.TextChanged += (s, ev) =>
        {
            searchTimer?.Stop();
            searchTimer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(300)
            };
            searchTimer.Tick += (_, __) =>
            {
                searchTimer.Stop();
                LoadDocuments(txtSearch.Text.Trim());
            };
            searchTimer.Start();
        };
        
        dialog.Content = mainPanel;
        dialog.ShowDialog();
    }
    
    /// <summary>
    /// Liên kết một Document từ module Quản lý văn bản vào cuộc họp
    /// </summary>
    private void LinkDocumentToMeeting(Document doc)
    {
        // Map DocumentType → MeetingDocumentType
        var meetingDocType = doc.Type switch
        {
            DocumentType.NghiQuyet => MeetingDocumentType.NghiQuyet,
            DocumentType.QuyetDinh => MeetingDocumentType.QuyetDinh,
            DocumentType.CongVan => MeetingDocumentType.CongVan,
            DocumentType.ThongBao => MeetingDocumentType.ThongBaoKetLuan,
            DocumentType.BaoCao or DocumentType.ToTrinh or DocumentType.KeHoach => MeetingDocumentType.TaiLieuHop,
            DocumentType.ChiThi or DocumentType.HuongDan => MeetingDocumentType.VanBanChiDao,
            _ => MeetingDocumentType.Khac
        };
        
        var meetingDoc = new MeetingDocument
        {
            DocumentType = meetingDocType,
            Title = doc.Title,
            DocumentNumber = doc.Number,
            IssuedDate = doc.IssueDate,
            Issuer = doc.Issuer,
            FilePath = doc.FilePath,
            LinkedDocumentId = doc.Id,
            Note = !string.IsNullOrEmpty(doc.Subject) ? doc.Subject : ""
        };
        
        _documents.Add(meetingDoc);
        RefreshDocumentList();
    }
    
    /// <summary>
    /// Mở popup xem chi tiết văn bản đã liên kết
    /// </summary>
    private void OpenLinkedDocumentViewer(string? linkedDocumentId)
    {
        if (string.IsNullOrEmpty(linkedDocumentId) || _documentService == null) return;
        
        var doc = _documentService.GetDocument(linkedDocumentId);
        if (doc == null)
        {
            MessageBox.Show(
                "Không tìm thấy văn bản liên kết trong hệ thống.\nVăn bản có thể đã bị xóa.",
                "Không tìm thấy",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }
        
        var viewer = new DocumentViewDialog(doc, _documentService)
        {
            Owner = this
        };
        viewer.ShowDialog();
    }
    
    private void RefreshDocumentList()
    {
        documentListPanel.Children.Clear();
        
        var hasDocuments = _documents.Count > 0;
        txtNoDocuments.Visibility = hasDocuments ? Visibility.Collapsed : Visibility.Visible;
        
        // Stats
        var invitationCount = _documents.Count(d => d.DocumentType == MeetingDocumentType.GiayMoi);
        txtDocStats.Text = hasDocuments ? $"({_documents.Count} văn bản" + (invitationCount == 0 ? " — ⚠️ chưa có giấy mời!" : "") + ")" : "";
        
        if (!hasDocuments) return;
        
        // Group documents by type, in a meaningful order
        var typeOrder = new[]
        {
            MeetingDocumentType.GiayMoi,
            MeetingDocumentType.ChuongTrinh,
            MeetingDocumentType.TaiLieuHop,
            MeetingDocumentType.BienBan,
            MeetingDocumentType.ThongBaoKetLuan,
            MeetingDocumentType.NghiQuyet,
            MeetingDocumentType.VanBanChiDao,
            MeetingDocumentType.QuyetDinh,
            MeetingDocumentType.CongVan,
            MeetingDocumentType.Khac
        };
        
        foreach (var docType in typeOrder)
        {
            var docsOfType = _documents.Where(d => d.DocumentType == docType).ToList();
            if (docsOfType.Count == 0) continue;
            
            // Type header
            var headerColor = MeetingHelper.GetDocumentTypeColor(docType);
            var headerPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 10, 0, 4) };
            headerPanel.Children.Add(new TextBlock
            {
                Text = $"{MeetingHelper.GetDocumentTypeIcon(docType)} {MeetingHelper.GetDocumentTypeName(docType).ToUpper()}",
                FontWeight = FontWeights.Bold,
                FontSize = 12,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(headerColor)),
                VerticalAlignment = VerticalAlignment.Center
            });
            
            if (docType == MeetingDocumentType.GiayMoi)
            {
                headerPanel.Children.Add(new TextBlock
                {
                    Text = " (bắt buộc)",
                    FontSize = 11,
                    Foreground = new SolidColorBrush(Color.FromRgb(0xE5, 0x39, 0x35)),
                    FontWeight = FontWeights.SemiBold,
                    VerticalAlignment = VerticalAlignment.Center
                });
            }
            
            headerPanel.Children.Add(new TextBlock
            {
                Text = $"  —  {docsOfType.Count} văn bản",
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(0x9E, 0x9E, 0x9E)),
                VerticalAlignment = VerticalAlignment.Center
            });
            
            documentListPanel.Children.Add(headerPanel);
            
            // Document cards for this type
            foreach (var doc in docsOfType)
            {
                var globalIndex = _documents.IndexOf(doc);
                var card = CreateDocumentCard(doc, globalIndex, headerColor);
                documentListPanel.Children.Add(card);
            }
        }
    }
    
    private UIElement CreateDocumentCard(MeetingDocument doc, int index, string accentColor)
    {
        var isLinked = !string.IsNullOrEmpty(doc.LinkedDocumentId);
        var hasFile = !string.IsNullOrEmpty(doc.FilePath) && System.IO.File.Exists(doc.FilePath);
        
        var card = new Border
        {
            BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(accentColor)),
            BorderThickness = new Thickness(3, 0, 0, 0),
            Background = isLinked
                ? new SolidColorBrush(Color.FromRgb(0xF1, 0xF8, 0xE9))  // Light green for linked
                : Brushes.White,
            Padding = new Thickness(12, 8, 8, 8),
            Margin = new Thickness(0, 0, 0, 4),
            CornerRadius = new CornerRadius(0, 6, 6, 0)
        };
        
        var mainPanel = new StackPanel();
        
        // === Linked document badge (if linked) ===
        if (isLinked)
        {
            var linkBadge = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(0xC8, 0xE6, 0xC9)),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(8, 3, 8, 3),
                Margin = new Thickness(0, 0, 0, 6),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            var linkPanel = new StackPanel { Orientation = Orientation.Horizontal };
            linkPanel.Children.Add(new TextBlock
            {
                Text = "🔗 Liên kết từ Quản lý văn bản",
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(0x2E, 0x7D, 0x32)),
                VerticalAlignment = VerticalAlignment.Center
            });
            
            // Unlink button
            var btnUnlink = new Button
            {
                Style = (Style)FindResource("MaterialDesignFlatButton"),
                Content = "✕ Hủy liên kết",
                FontSize = 10,
                Height = 20,
                Padding = new Thickness(4, 0, 4, 0),
                Margin = new Thickness(8, 0, 0, 0),
                Foreground = new SolidColorBrush(Color.FromRgb(0xE5, 0x39, 0x35)),
                VerticalAlignment = VerticalAlignment.Center
            };
            var capturedIdxUnlink = index;
            btnUnlink.Click += (s, ev) =>
            {
                if (capturedIdxUnlink < _documents.Count)
                {
                    _documents[capturedIdxUnlink].LinkedDocumentId = string.Empty;
                    RefreshDocumentList();
                }
            };
            linkPanel.Children.Add(btnUnlink);
            
            // View document button
            if (_documentService != null)
            {
                var btnViewDoc = new Button
                {
                    Style = (Style)FindResource("MaterialDesignFlatButton"),
                    Content = "👁 Xem văn bản",
                    FontSize = 10,
                    Height = 20,
                    Padding = new Thickness(4, 0, 4, 0),
                    Margin = new Thickness(4, 0, 0, 0),
                    Foreground = new SolidColorBrush(Color.FromRgb(0x15, 0x65, 0xC0)),
                    VerticalAlignment = VerticalAlignment.Center
                };
                var capturedLinkedId = doc.LinkedDocumentId;
                btnViewDoc.Click += (s, ev) =>
                {
                    OpenLinkedDocumentViewer(capturedLinkedId);
                };
                linkPanel.Children.Add(btnViewDoc);
            }
            
            linkBadge.Child = linkPanel;
            mainPanel.Children.Add(linkBadge);
        }
        
        // === Row 1: Title + Number + Action buttons ===
        var row1 = new Grid { Margin = new Thickness(0, 0, 0, 4) };
        row1.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        row1.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8) });
        row1.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200) });
        row1.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        
        // Title
        var txtDocTitle = new TextBox { Text = doc.Title, FontSize = 13 };
        HintAssist.SetHint(txtDocTitle, "Trích yếu / Tên văn bản");
        var capturedIndex1 = index;
        txtDocTitle.TextChanged += (s, ev) => { if (capturedIndex1 < _documents.Count) _documents[capturedIndex1].Title = txtDocTitle.Text; };
        Grid.SetColumn(txtDocTitle, 0);
        row1.Children.Add(txtDocTitle);
        
        // Document Number
        var txtDocNumber = new TextBox { Text = doc.DocumentNumber, FontSize = 13, FontWeight = FontWeights.SemiBold };
        HintAssist.SetHint(txtDocNumber, "Số hiệu VB (VD: 15/GM-UBND)");
        var capturedIndex2 = index;
        txtDocNumber.TextChanged += (s, ev) => { if (capturedIndex2 < _documents.Count) _documents[capturedIndex2].DocumentNumber = txtDocNumber.Text; };
        Grid.SetColumn(txtDocNumber, 2);
        row1.Children.Add(txtDocNumber);
        
        // Action buttons panel
        var actionPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(4, 0, 0, 0) };
        
        // Link to document button (only if DocumentService available and not already linked)
        if (_documentService != null && !isLinked)
        {
            var btnLink = new Button
            {
                Style = (Style)FindResource("MaterialDesignIconButton"),
                Width = 28, Height = 28,
                ToolTip = "Liên kết đến văn bản trong hệ thống",
                Foreground = new SolidColorBrush(Color.FromRgb(0x15, 0x65, 0xC0))
            };
            btnLink.Content = new PackIcon { Kind = PackIconKind.LinkVariant, Width = 16, Height = 16 };
            btnLink.Click += (s, ev) => ShowDocumentPickerDialog();
            actionPanel.Children.Add(btnLink);
        }
        
        // Delete button
        var btnDel = new Button
        {
            Style = (Style)FindResource("MaterialDesignIconButton"),
            Width = 28, Height = 28,
            ToolTip = "Xóa văn bản",
            Foreground = new SolidColorBrush(Color.FromRgb(0xE5, 0x39, 0x35))
        };
        btnDel.Content = new PackIcon { Kind = PackIconKind.Close, Width = 14, Height = 14 };
        var capturedIndexDel = index;
        btnDel.Click += (s, ev) =>
        {
            if (capturedIndexDel < _documents.Count)
            {
                _documents.RemoveAt(capturedIndexDel);
                RefreshDocumentList();
            }
        };
        actionPanel.Children.Add(btnDel);
        
        Grid.SetColumn(actionPanel, 3);
        row1.Children.Add(actionPanel);
        
        mainPanel.Children.Add(row1);
        
        // === Row 2: Issued date + Issuer ===
        var row2 = new Grid { Margin = new Thickness(0, 0, 0, 2) };
        row2.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(140) });
        row2.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(6) });
        row2.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        
        // Issued Date
        var dpIssued = new DatePicker { SelectedDate = doc.IssuedDate, FontSize = 11 };
        HintAssist.SetHint(dpIssued, "Ngày ban hành");
        var capturedIndex3 = index;
        dpIssued.SelectedDateChanged += (s, ev) => { if (capturedIndex3 < _documents.Count) _documents[capturedIndex3].IssuedDate = dpIssued.SelectedDate; };
        Grid.SetColumn(dpIssued, 0);
        row2.Children.Add(dpIssued);
        
        // Issuer
        var txtIssuer = new TextBox { Text = doc.Issuer, FontSize = 12 };
        HintAssist.SetHint(txtIssuer, "Cơ quan ban hành");
        var capturedIndex4 = index;
        txtIssuer.TextChanged += (s, ev) => { if (capturedIndex4 < _documents.Count) _documents[capturedIndex4].Issuer = txtIssuer.Text; };
        Grid.SetColumn(txtIssuer, 2);
        row2.Children.Add(txtIssuer);
        
        mainPanel.Children.Add(row2);
        
        // === Row 3: File management bar ===
        var fileBar = new Grid { Margin = new Thickness(0, 4, 0, 0) };
        fileBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        fileBar.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        fileBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        
        // File status with icon
        var fileStatusPanel = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
        
        if (!string.IsNullOrEmpty(doc.FilePath))
        {
            var ext = System.IO.Path.GetExtension(doc.FilePath).ToLower();
            var (fileIcon, fileColor) = ext switch
            {
                ".docx" or ".doc" => ("📄", "#1565C0"),
                ".pdf" => ("📕", "#C62828"),
                ".xlsx" or ".xls" => ("📊", "#2E7D32"),
                ".pptx" or ".ppt" => ("📙", "#E65100"),
                ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" => ("🖼️", "#6A1B9A"),
                ".zip" or ".rar" or ".7z" => ("📦", "#795548"),
                _ => ("📎", "#616161")
            };
            
            fileStatusPanel.Children.Add(new TextBlock
            {
                Text = fileIcon,
                FontSize = 14,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 4, 0)
            });
            
            var fileName = System.IO.Path.GetFileName(doc.FilePath);
            fileStatusPanel.Children.Add(new TextBlock
            {
                Text = fileName,
                FontSize = 11,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(fileColor)),
                FontWeight = FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center,
                MaxWidth = 280,
                TextTrimming = TextTrimming.CharacterEllipsis,
                ToolTip = doc.FilePath
            });
            
            // File size
            try
            {
                if (System.IO.File.Exists(doc.FilePath))
                {
                    var fileInfo = new System.IO.FileInfo(doc.FilePath);
                    var sizeText = fileInfo.Length < 1024 * 1024
                        ? $" ({fileInfo.Length / 1024} KB)"
                        : $" ({fileInfo.Length / (1024 * 1024.0):F1} MB)";
                    fileStatusPanel.Children.Add(new TextBlock
                    {
                        Text = sizeText,
                        FontSize = 10,
                        Foreground = new SolidColorBrush(Color.FromRgb(0x9E, 0x9E, 0x9E)),
                        VerticalAlignment = VerticalAlignment.Center
                    });
                }
            }
            catch { /* ignore file access errors */ }
        }
        else
        {
            fileStatusPanel.Children.Add(new TextBlock
            {
                Text = "📌 Chưa đính kèm file",
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(0x9E, 0x9E, 0x9E)),
                FontStyle = FontStyles.Italic,
                VerticalAlignment = VerticalAlignment.Center
            });
        }
        
        Grid.SetColumn(fileStatusPanel, 0);
        fileBar.Children.Add(fileStatusPanel);
        
        // File action buttons (right)
        var fileActions = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
        
        // Open file button (only if file exists)
        if (hasFile)
        {
            var btnOpen = new Button
            {
                Style = (Style)FindResource("MaterialDesignOutlinedButton"),
                Padding = new Thickness(6, 2, 6, 2),
                FontSize = 11,
                Height = 26,
                Margin = new Thickness(0, 0, 4, 0),
                ToolTip = "Mở file bằng ứng dụng mặc định"
            };
            var openPanel = new StackPanel { Orientation = Orientation.Horizontal };
            openPanel.Children.Add(new PackIcon { Kind = PackIconKind.OpenInNew, Width = 13, Height = 13, Margin = new Thickness(0, 0, 4, 0), VerticalAlignment = VerticalAlignment.Center });
            openPanel.Children.Add(new TextBlock { Text = "Mở file", FontSize = 11, VerticalAlignment = VerticalAlignment.Center });
            btnOpen.Content = openPanel;
            btnOpen.Click += (s, ev) =>
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = doc.FilePath,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Không thể mở file:\n{ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };
            fileActions.Children.Add(btnOpen);
            
            // Open folder button
            var btnFolder = new Button
            {
                Style = (Style)FindResource("MaterialDesignIconButton"),
                Width = 26, Height = 26,
                ToolTip = "Mở thư mục chứa file",
                Margin = new Thickness(0, 0, 4, 0)
            };
            btnFolder.Content = new PackIcon { Kind = PackIconKind.FolderOpen, Width = 13, Height = 13 };
            btnFolder.Click += (s, ev) =>
            {
                try
                {
                    System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{doc.FilePath}\"");
                }
                catch { }
            };
            fileActions.Children.Add(btnFolder);
        }
        
        // Choose/change file button
        var btnPickFile = new Button
        {
            Style = (Style)FindResource("MaterialDesignOutlinedButton"),
            Padding = new Thickness(6, 2, 6, 2),
            FontSize = 11,
            Height = 26,
            ToolTip = string.IsNullOrEmpty(doc.FilePath) ? "Đính kèm file" : "Đổi file khác"
        };
        var pickPanel = new StackPanel { Orientation = Orientation.Horizontal };
        pickPanel.Children.Add(new PackIcon
        {
            Kind = string.IsNullOrEmpty(doc.FilePath) ? PackIconKind.PaperclipPlus : PackIconKind.SwapHorizontal,
            Width = 13, Height = 13,
            Margin = new Thickness(0, 0, 4, 0),
            VerticalAlignment = VerticalAlignment.Center
        });
        pickPanel.Children.Add(new TextBlock
        {
            Text = string.IsNullOrEmpty(doc.FilePath) ? "Đính kèm" : "Đổi file",
            FontSize = 11,
            VerticalAlignment = VerticalAlignment.Center
        });
        btnPickFile.Content = pickPanel;
        
        var capturedIndex5 = index;
        btnPickFile.Click += (s, ev) =>
        {
            var dlg = new OpenFileDialog
            {
                Title = "Chọn file văn bản",
                Filter = "Tất cả file|*.*|Word|*.docx;*.doc|PDF|*.pdf|Excel|*.xlsx;*.xls|PowerPoint|*.pptx;*.ppt|Ảnh|*.jpg;*.png;*.gif"
            };
            if (dlg.ShowDialog() == true && capturedIndex5 < _documents.Count)
            {
                _documents[capturedIndex5].FilePath = dlg.FileName;
                RefreshDocumentList();
            }
        };
        fileActions.Children.Add(btnPickFile);
        
        // Remove file button (if has file)
        if (!string.IsNullOrEmpty(doc.FilePath))
        {
            var btnRemoveFile = new Button
            {
                Style = (Style)FindResource("MaterialDesignIconButton"),
                Width = 26, Height = 26,
                ToolTip = "Xóa file đính kèm",
                Foreground = new SolidColorBrush(Color.FromRgb(0xE5, 0x39, 0x35))
            };
            btnRemoveFile.Content = new PackIcon { Kind = PackIconKind.PaperclipOff, Width = 13, Height = 13 };
            var capturedIdxRemove = index;
            btnRemoveFile.Click += (s, ev) =>
            {
                if (capturedIdxRemove < _documents.Count)
                {
                    _documents[capturedIdxRemove].FilePath = string.Empty;
                    RefreshDocumentList();
                }
            };
            fileActions.Children.Add(btnRemoveFile);
        }
        
        Grid.SetColumn(fileActions, 2);
        fileBar.Children.Add(fileActions);
        
        mainPanel.Children.Add(fileBar);
        
        // === Row 4: Note (compact) ===
        var txtNote = new TextBox { Text = doc.Note, FontSize = 11, Margin = new Thickness(0, 4, 0, 0) };
        HintAssist.SetHint(txtNote, "Ghi chú");
        var capturedIndex6 = index;
        txtNote.TextChanged += (s, ev) => { if (capturedIndex6 < _documents.Count) _documents[capturedIndex6].Note = txtNote.Text; };
        mainPanel.Children.Add(txtNote);
        
        card.Child = mainPanel;
        return card;
    }
    
    #endregion
    
    #region Attachment Management
    
    private void AddAttachment_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Chọn file đính kèm",
            Multiselect = true,
            Filter = "Tất cả file|*.*|Word|*.docx;*.doc|PDF|*.pdf|Excel|*.xlsx;*.xls|PowerPoint|*.pptx;*.ppt|Ảnh|*.jpg;*.png;*.gif"
        };
        
        if (dialog.ShowDialog() == true)
        {
            foreach (var file in dialog.FileNames)
            {
                if (!_attachmentPaths.Contains(file))
                    _attachmentPaths.Add(file);
            }
            RefreshAttachmentList();
        }
    }
    
    private void RefreshAttachmentList()
    {
        attachmentListPanel.Children.Clear();
        txtNoAttachments.Visibility = _attachmentPaths.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        
        foreach (var path in _attachmentPaths)
        {
            var index = _attachmentPaths.IndexOf(path);
            var fileName = System.IO.Path.GetFileName(path);
            var ext = System.IO.Path.GetExtension(path).ToLower();
            
            var icon = ext switch
            {
                ".docx" or ".doc" => "📄",
                ".pdf" => "📕",
                ".xlsx" or ".xls" => "📊",
                ".pptx" or ".ppt" => "📽️",
                ".jpg" or ".png" or ".gif" or ".bmp" => "🖼️",
                _ => "📎"
            };
            
            var row = new DockPanel { Margin = new Thickness(0, 0, 0, 4) };
            
            var btnRemove = new Button
            {
                Style = (Style)FindResource("MaterialDesignIconButton"),
                Width = 24, Height = 24,
                ToolTip = "Xóa"
            };
            btnRemove.Content = new PackIcon { Kind = PackIconKind.Close, Width = 14, Height = 14 };
            var capturedPath = path;
            btnRemove.Click += (s, ev) =>
            {
                _attachmentPaths.Remove(capturedPath);
                RefreshAttachmentList();
            };
            DockPanel.SetDock(btnRemove, Dock.Right);
            row.Children.Add(btnRemove);
            
            var text = new TextBlock
            {
                Text = $"{icon} {fileName}",
                FontSize = 12,
                VerticalAlignment = VerticalAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis,
                ToolTip = path
            };
            row.Children.Add(text);
            
            attachmentListPanel.Children.Add(row);
        }
    }
    
    #endregion
    
    #region Event Handlers
    
    private void CboFormat_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (cboFormat.SelectedItem is ComboBoxItem item && item.Tag is MeetingFormat format)
        {
            txtOnlineLink.Visibility = format != MeetingFormat.TrucTiep ? Visibility.Visible : Visibility.Collapsed;
        }
    }
    
    private void Save_Click(object sender, RoutedEventArgs e)
    {
        // Validate
        if (string.IsNullOrWhiteSpace(txtTitle.Text))
        {
            MessageBox.Show("Vui lòng nhập tên cuộc họp!", "Thiếu thông tin",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            tabControl.SelectedIndex = 0;
            txtTitle.Focus();
            return;
        }
        
        if (dpStartDate.SelectedDate == null)
        {
            MessageBox.Show("Vui lòng chọn ngày bắt đầu!", "Thiếu thông tin",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            tabControl.SelectedIndex = 0;
            dpStartDate.Focus();
            return;
        }
        
        try
        {
            // Collect data from all tabs
            
            // Tab 1: Thông tin chung
            _meeting.Title = txtTitle.Text.Trim();
            _meeting.MeetingNumber = txtMeetingNumber.Text.Trim();
            
            if (cboMeetingType.SelectedItem is ComboBoxItem typeItem && typeItem.Tag is MeetingType type)
                _meeting.Type = type;
            if (cboLevel.SelectedItem is ComboBoxItem levelItem && levelItem.Tag is MeetingLevel level)
                _meeting.Level = level;
            if (cboStatus.SelectedItem is ComboBoxItem statusItem && statusItem.Tag is MeetingStatus status)
                _meeting.Status = status;
            if (cboFormat.SelectedItem is ComboBoxItem formatItem && formatItem.Tag is MeetingFormat format)
                _meeting.Format = format;
            if (cboPriority.SelectedItem is ComboBoxItem prioItem)
                _meeting.Priority = int.TryParse(prioItem.Tag?.ToString(), out var p) ? p : 3;
            
            // Build StartTime
            var startDate = dpStartDate.SelectedDate!.Value;
            if (tpStartTime.SelectedTime.HasValue)
            {
                var time = tpStartTime.SelectedTime.Value;
                _meeting.StartTime = new DateTime(startDate.Year, startDate.Month, startDate.Day,
                    time.Hour, time.Minute, 0);
            }
            else
            {
                _meeting.StartTime = startDate.Date.AddHours(8);
            }
            
            // Build EndTime
            if (dpEndDate.SelectedDate.HasValue)
            {
                var endDate = dpEndDate.SelectedDate.Value;
                if (tpEndTime.SelectedTime.HasValue)
                {
                    var time = tpEndTime.SelectedTime.Value;
                    _meeting.EndTime = new DateTime(endDate.Year, endDate.Month, endDate.Day,
                        time.Hour, time.Minute, 0);
                }
                else
                {
                    _meeting.EndTime = endDate.Date.AddHours(17);
                }
            }
            else if (tpEndTime.SelectedTime.HasValue)
            {
                // End time on same day
                var time = tpEndTime.SelectedTime.Value;
                _meeting.EndTime = new DateTime(startDate.Year, startDate.Month, startDate.Day,
                    time.Hour, time.Minute, 0);
            }
            else
            {
                _meeting.EndTime = null;
            }
            
            _meeting.IsAllDay = chkAllDay.IsChecked ?? false;
            _meeting.Location = txtLocation.Text.Trim();
            _meeting.OnlineLink = txtOnlineLink.Text.Trim();
            
            // Tags
            _meeting.Tags = string.IsNullOrWhiteSpace(txtTags.Text) 
                ? Array.Empty<string>()
                : txtTags.Text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            
            // Tab 2: Thành phần
            _meeting.OrganizingUnit = txtOrganizingUnit.Text.Trim();
            _meeting.ChairPerson = txtChairPerson.Text.Trim();
            _meeting.ChairPersonTitle = txtChairPersonTitle.Text.Trim();
            _meeting.Secretary = txtSecretary.Text.Trim();
            _meeting.Attendees = _attendees.Where(a => !string.IsNullOrWhiteSpace(a.Name)).ToList();
            
            // Tab 3: Nội dung
            _meeting.Agenda = txtAgenda.Text.Trim();
            _meeting.Content = txtContent.Text.Trim();
            _meeting.Conclusion = txtConclusion.Text.Trim();
            _meeting.PersonalNotes = txtPersonalNotes.Text.Trim();
            
            // Tab 4: Nhiệm vụ
            _meeting.Tasks = _tasks.Where(t => !string.IsNullOrWhiteSpace(t.Title)).ToList();
            
            // Tab 5: Văn bản cuộc họp
            _meeting.Documents = _documents.Where(d => !string.IsNullOrWhiteSpace(d.Title) || !string.IsNullOrWhiteSpace(d.DocumentNumber) || !string.IsNullOrWhiteSpace(d.FilePath)).ToList();
            _meeting.AttachmentPaths = _attachmentPaths.ToArray();
            
            // Tab 6: Album ảnh liên quan
            _meeting.RelatedAlbumIds = _relatedAlbumIds.ToArray();
            
            // Sync legacy fields for backward compatibility
            var invitation = _meeting.Documents.FirstOrDefault(d => d.DocumentType == MeetingDocumentType.GiayMoi);
            _meeting.InvitationDocId = invitation?.DocumentNumber ?? "";
            var minutes = _meeting.Documents.FirstOrDefault(d => d.DocumentType == MeetingDocumentType.BienBan);
            _meeting.MinutesDocId = minutes?.DocumentNumber ?? "";
            var conclusion = _meeting.Documents.FirstOrDefault(d => d.DocumentType == MeetingDocumentType.ThongBaoKetLuan);
            _meeting.ConclusionDocId = conclusion?.DocumentNumber ?? "";
            
            // Save
            if (_isNew)
            {
                _meetingService.AddMeeting(_meeting);
            }
            else
            {
                _meetingService.UpdateMeeting(_meeting);
            }
            
            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi lưu cuộc họp:\n{ex.Message}", "Lỗi",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
    
    #endregion
    
    #region Tab 6: Album ảnh liên quan
    
    private void LinkAlbum_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var allAlbums = _albumService.GetAllAlbums();
            if (allAlbums.Count == 0)
            {
                MessageBox.Show("Chưa có album ảnh nào trong hệ thống.\nHãy tạo album trong mục 'Album ảnh' trước.",
                    "Chưa có album", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            
            // Filter out already linked albums
            var available = allAlbums.Where(a => !_relatedAlbumIds.Contains(a.Id)).ToList();
            if (available.Count == 0)
            {
                MessageBox.Show("Tất cả album đã được liên kết!", "Thông báo", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            
            // Show album picker dialog
            var picker = new Window
            {
                Title = "Chọn album ảnh để liên kết",
                Width = 680,
                Height = 520,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                Background = (Brush)FindResource("MaterialDesignPaper")
            };
            
            var mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            
            // Header
            var header = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(0xE3, 0xF2, 0xFD)),
                Padding = new Thickness(16, 12, 16, 12)
            };
            var headerPanel = new StackPanel { Orientation = Orientation.Horizontal };
            headerPanel.Children.Add(new PackIcon { Kind = PackIconKind.ImageMultiple, Width = 22, Height = 22, 
                VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 8, 0),
                Foreground = new SolidColorBrush(Color.FromRgb(0x15, 0x65, 0xC0)) });
            headerPanel.Children.Add(new TextBlock { Text = "CHỌN ALBUM ẢNH", FontSize = 15, FontWeight = FontWeights.Bold,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Color.FromRgb(0x15, 0x65, 0xC0)) });
            headerPanel.Children.Add(new TextBlock { Text = $"  ({available.Count} album khả dụng)", FontSize = 12,
                VerticalAlignment = VerticalAlignment.Center, Opacity = 0.7 });
            header.Child = headerPanel;
            Grid.SetRow(header, 0);
            
            // Search box
            var searchBox = new TextBox
            {
                Margin = new Thickness(16, 10, 16, 6),
                FontSize = 13
            };
            HintAssist.SetHint(searchBox, "🔍 Tìm kiếm album...");
            HintAssist.SetIsFloating(searchBox, false);
            Grid.SetRow(searchBox, 1);
            
            // Album list
            var listBox = new ListBox
            {
                Margin = new Thickness(16, 4, 16, 4),
                SelectionMode = SelectionMode.Multiple,
                MaxHeight = 340
            };
            
            foreach (var album in available.OrderByDescending(a => a.ModifiedDate))
            {
                var itemBorder = new Border
                {
                    Padding = new Thickness(10, 8, 10, 8),
                    Margin = new Thickness(0, 2, 0, 2),
                    CornerRadius = new CornerRadius(6),
                    Background = new SolidColorBrush(Color.FromRgb(0xFA, 0xFA, 0xFA))
                };
                
                var itemGrid = new Grid();
                itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
                itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                
                // Album thumbnail
                var thumbBorder = new Border
                {
                    Width = 52, Height = 52,
                    CornerRadius = new CornerRadius(6),
                    Background = new SolidColorBrush(Color.FromRgb(0xE0, 0xE0, 0xE0)),
                    Margin = new Thickness(0, 0, 8, 0),
                    ClipToBounds = true
                };
                
                if (album.ThumbnailPhotos.Count > 0 && System.IO.File.Exists(album.ThumbnailPhotos[0]))
                {
                    try
                    {
                        var bitmap = new System.Windows.Media.Imaging.BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(album.ThumbnailPhotos[0]);
                        bitmap.DecodePixelWidth = 80;
                        bitmap.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        bitmap.Freeze();
                        
                        thumbBorder.Background = new ImageBrush(bitmap)
                        {
                            Stretch = Stretch.UniformToFill
                        };
                    }
                    catch { /* skip thumbnail error */ }
                }
                else
                {
                    var iconPackIcon = new PackIcon
                    {
                        Kind = PackIconKind.ImageOutline,
                        Width = 24, Height = 24,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Foreground = new SolidColorBrush(Color.FromRgb(0x9E, 0x9E, 0x9E))
                    };
                    thumbBorder.Child = iconPackIcon;
                }
                Grid.SetColumn(thumbBorder, 0);
                
                // Album info
                var infoPanel = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
                infoPanel.Children.Add(new TextBlock
                {
                    Text = album.Title,
                    FontSize = 13,
                    FontWeight = FontWeights.SemiBold,
                    TextTrimming = TextTrimming.CharacterEllipsis
                });
                
                var detailPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 3, 0, 0) };
                detailPanel.Children.Add(new TextBlock
                {
                    Text = $"📷 {album.PhotoCount} ảnh",
                    FontSize = 11,
                    Foreground = new SolidColorBrush(Color.FromRgb(0x75, 0x75, 0x75)),
                    Margin = new Thickness(0, 0, 12, 0)
                });
                detailPanel.Children.Add(new TextBlock
                {
                    Text = $"📅 {album.ModifiedDate:dd/MM/yyyy}",
                    FontSize = 11,
                    Foreground = new SolidColorBrush(Color.FromRgb(0x75, 0x75, 0x75))
                });
                infoPanel.Children.Add(detailPanel);
                
                if (!string.IsNullOrEmpty(album.AlbumFolderPath))
                {
                    infoPanel.Children.Add(new TextBlock
                    {
                        Text = $"📁 {album.AlbumFolderPath}",
                        FontSize = 10.5,
                        Foreground = new SolidColorBrush(Color.FromRgb(0x9E, 0x9E, 0x9E)),
                        Margin = new Thickness(0, 2, 0, 0),
                        TextTrimming = TextTrimming.CharacterEllipsis
                    });
                }
                
                Grid.SetColumn(infoPanel, 1);
                
                itemGrid.Children.Add(thumbBorder);
                itemGrid.Children.Add(infoPanel);
                itemBorder.Child = itemGrid;
                
                var lbi = new ListBoxItem { Content = itemBorder, Tag = album, Padding = new Thickness(4) };
                listBox.Items.Add(lbi);
            }
            
            // Search filter
            searchBox.TextChanged += (s, ev) =>
            {
                var keyword = searchBox.Text?.Trim().ToLower() ?? "";
                foreach (ListBoxItem item in listBox.Items)
                {
                    if (item.Tag is SimpleAlbum a)
                    {
                        item.Visibility = string.IsNullOrEmpty(keyword) || 
                            a.Title.ToLower().Contains(keyword) ||
                            (a.AlbumFolderPath?.ToLower().Contains(keyword) ?? false) ||
                            a.Tags.Any(t => t.ToLower().Contains(keyword))
                            ? Visibility.Visible : Visibility.Collapsed;
                    }
                }
            };
            
            Grid.SetRow(listBox, 2);
            
            // Buttons
            var btnPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(16, 10, 16, 14)
            };
            var btnOk = new Button
            {
                Content = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Children = { 
                        new PackIcon { Kind = PackIconKind.LinkPlus, Width = 16, Height = 16, 
                            VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0,0,6,0) },
                        new TextBlock { Text = "Liên kết album đã chọn", VerticalAlignment = VerticalAlignment.Center }
                    }
                },
                Padding = new Thickness(16, 6, 16, 6),
                Margin = new Thickness(0, 0, 10, 0),
                IsDefault = true,
                Style = (Style)FindResource("MaterialDesignRaisedButton")
            };
            var btnCancel = new Button
            {
                Content = "Đóng",
                Padding = new Thickness(16, 6, 16, 6),
                IsCancel = true,
                Style = (Style)FindResource("MaterialDesignOutlinedButton")
            };
            
            btnOk.Click += (s, ev) =>
            {
                var selected = listBox.Items.Cast<ListBoxItem>()
                    .Where(i => i.IsSelected && i.Tag is SimpleAlbum)
                    .Select(i => ((SimpleAlbum)i.Tag).Id)
                    .ToList();
                    
                if (selected.Count == 0)
                {
                    MessageBox.Show("Vui lòng chọn ít nhất 1 album!", "Chưa chọn", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                foreach (var id in selected)
                {
                    if (!_relatedAlbumIds.Contains(id))
                        _relatedAlbumIds.Add(id);
                }
                
                RefreshLinkedAlbumList();
                picker.Close();
            };
            
            btnCancel.Click += (s, ev) => picker.Close();
            
            // Double-click to quickly add
            listBox.MouseDoubleClick += (s, ev) =>
            {
                if (listBox.SelectedItem is ListBoxItem selectedItem && selectedItem.Tag is SimpleAlbum selectedAlbum)
                {
                    if (!_relatedAlbumIds.Contains(selectedAlbum.Id))
                    {
                        _relatedAlbumIds.Add(selectedAlbum.Id);
                        RefreshLinkedAlbumList();
                    }
                    // Remove from picker list
                    listBox.Items.Remove(selectedItem);
                    
                    // Update header count
                    var remaining = listBox.Items.Cast<ListBoxItem>().Count(i => i.Visibility == Visibility.Visible);
                    if (remaining == 0)
                        picker.Close();
                }
            };
            
            btnPanel.Children.Add(btnOk);
            btnPanel.Children.Add(btnCancel);
            Grid.SetRow(btnPanel, 3);
            
            mainGrid.Children.Add(header);
            mainGrid.Children.Add(searchBox);
            mainGrid.Children.Add(listBox);
            mainGrid.Children.Add(btnPanel);
            picker.Content = mainGrid;
            
            picker.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi tải danh sách album:\n{ex.Message}", "Lỗi",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    private void RefreshLinkedAlbumList()
    {
        linkedAlbumListPanel.Children.Clear();
        
        txtNoLinkedAlbums.Visibility = _relatedAlbumIds.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        txtAlbumStats.Text = _relatedAlbumIds.Count > 0 ? $"— {_relatedAlbumIds.Count} album" : "";
        
        foreach (var albumId in _relatedAlbumIds.ToList())
        {
            var album = _albumService.GetAlbumById(albumId);
            
            var card = new Border
            {
                Margin = new Thickness(0, 4, 0, 4),
                Padding = new Thickness(12, 10, 12, 10),
                CornerRadius = new CornerRadius(8),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0xE0, 0xE0, 0xE0)),
                BorderThickness = new Thickness(1),
                Background = new SolidColorBrush(Color.FromRgb(0xFA, 0xFA, 0xFA))
            };
            
            var cardGrid = new Grid();
            cardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(72) });
            cardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            cardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            
            // Thumbnail (2x2 grid or single cover)
            var thumbBorder = new Border
            {
                Width = 64, Height = 64,
                CornerRadius = new CornerRadius(8),
                Background = new SolidColorBrush(Color.FromRgb(0xE8, 0xE8, 0xE8)),
                ClipToBounds = true,
                Margin = new Thickness(0, 0, 10, 0)
            };
            
            if (album != null && album.ThumbnailPhotos.Count > 0 && System.IO.File.Exists(album.ThumbnailPhotos[0]))
            {
                try
                {
                    var bitmap = new System.Windows.Media.Imaging.BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(album.ThumbnailPhotos[0]);
                    bitmap.DecodePixelWidth = 100;
                    bitmap.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    
                    thumbBorder.Background = new ImageBrush(bitmap) { Stretch = Stretch.UniformToFill };
                }
                catch { /* skip */ }
            }
            else
            {
                thumbBorder.Child = new PackIcon
                {
                    Kind = PackIconKind.ImageMultiple,
                    Width = 28, Height = 28,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = new SolidColorBrush(Color.FromRgb(0x9E, 0x9E, 0x9E))
                };
            }
            Grid.SetColumn(thumbBorder, 0);
            
            // Album info
            var infoPanel = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            
            if (album != null)
            {
                infoPanel.Children.Add(new TextBlock
                {
                    Text = album.Title,
                    FontSize = 14,
                    FontWeight = FontWeights.SemiBold,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    Margin = new Thickness(0, 0, 0, 3)
                });
                
                var metaPanel = new WrapPanel();
                metaPanel.Children.Add(new TextBlock
                {
                    Text = $"📷 {album.PhotoCount} ảnh",
                    FontSize = 11.5,
                    Foreground = new SolidColorBrush(Color.FromRgb(0x61, 0x61, 0x61)),
                    Margin = new Thickness(0, 0, 14, 0)
                });
                metaPanel.Children.Add(new TextBlock
                {
                    Text = $"📅 {album.ModifiedDate:dd/MM/yyyy}",
                    FontSize = 11.5,
                    Foreground = new SolidColorBrush(Color.FromRgb(0x61, 0x61, 0x61)),
                    Margin = new Thickness(0, 0, 14, 0)
                });
                if (!string.IsNullOrEmpty(album.AlbumFolderPath))
                {
                    metaPanel.Children.Add(new TextBlock
                    {
                        Text = $"📁 {album.AlbumFolderPath}",
                        FontSize = 11,
                        Foreground = new SolidColorBrush(Color.FromRgb(0x9E, 0x9E, 0x9E)),
                        TextTrimming = TextTrimming.CharacterEllipsis,
                        MaxWidth = 300
                    });
                }
                infoPanel.Children.Add(metaPanel);
                
                if (!string.IsNullOrEmpty(album.Description))
                {
                    var desc = album.Description.Length > 80 ? album.Description.Substring(0, 80) + "..." : album.Description;
                    infoPanel.Children.Add(new TextBlock
                    {
                        Text = desc,
                        FontSize = 11,
                        Foreground = new SolidColorBrush(Color.FromRgb(0x9E, 0x9E, 0x9E)),
                        FontStyle = FontStyles.Italic,
                        Margin = new Thickness(0, 3, 0, 0),
                        TextTrimming = TextTrimming.CharacterEllipsis
                    });
                }
            }
            else
            {
                // Album deleted or not found
                infoPanel.Children.Add(new TextBlock
                {
                    Text = $"⚠️ Album không tìm thấy (ID: {albumId.Substring(0, Math.Min(8, albumId.Length))}...)",
                    FontSize = 12,
                    Foreground = new SolidColorBrush(Color.FromRgb(0xE6, 0x51, 0x00)),
                    FontStyle = FontStyles.Italic
                });
            }
            Grid.SetColumn(infoPanel, 1);
            
            // Remove button
            var capturedAlbumId = albumId;
            var removeBtn = new Button
            {
                ToolTip = "Gỡ liên kết album này",
                Width = 32, Height = 32,
                Padding = new Thickness(0),
                Style = (Style)FindResource("MaterialDesignIconButton"),
                Foreground = new SolidColorBrush(Color.FromRgb(0xC6, 0x28, 0x28)),
                Content = new PackIcon { Kind = PackIconKind.LinkOff, Width = 18, Height = 18 },
                VerticalAlignment = VerticalAlignment.Center
            };
            removeBtn.Click += (s, ev) =>
            {
                _relatedAlbumIds.Remove(capturedAlbumId);
                RefreshLinkedAlbumList();
            };
            Grid.SetColumn(removeBtn, 2);
            
            cardGrid.Children.Add(thumbBorder);
            cardGrid.Children.Add(infoPanel);
            cardGrid.Children.Add(removeBtn);
            card.Child = cardGrid;
            
            linkedAlbumListPanel.Children.Add(card);
        }
    }
    
    #endregion
}
