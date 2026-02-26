using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Windows;
using AIVanBan.Core.Models;
using AIVanBan.Core.Services;
using AIVanBan.Desktop.Services;
using AIVanBan.Desktop.Views;

namespace AIVanBan.Desktop;

public partial class MainWindow : Window
{
    private readonly DocumentService _documentService;
    private readonly AlbumStructureService _albumService;
    private bool _isSidebarCollapsed = false;
    
    public MainWindow()
    {
        try
        {
            InitializeComponent();
            
            Console.WriteLine("🔧 Initializing DocumentService...");
            _documentService = new DocumentService();
            
            Console.WriteLine("🔧 Initializing AlbumStructureService...");
            _albumService = new AlbumStructureService();
            
            // Initialize album templates AFTER DocumentService is fully initialized
            Console.WriteLine("🔧 Initializing album templates...");
            _albumService.InitializeDefaultTemplates();
            
            // Seed default document templates if needed
            Console.WriteLine("🔧 Seeding default data...");
            InitializeDefaultData();
            
            // Unified first-run setup — tạo cả thư mục tài liệu + album ảnh
            Console.WriteLine("🔧 Checking first-run setup...");
            CheckFirstRunSetup();
            
            Console.WriteLine("🔧 Loading statistics...");
            LoadStatistics();
            
            Console.WriteLine("🔧 Loading API status bar...");
            LoadApiStatusBar();
            
            // Cập nhật trạng thái sidebar AI buttons
            Console.WriteLine("🔧 Updating AI sidebar state...");
            UpdateAiSidebarState();
            
            // Navigate to Dashboard on startup
            Console.WriteLine("🔧 Loading Dashboard...");
            WelcomeScreen.Visibility = Visibility.Collapsed;
            MainFrame.Navigate(new Views.DashboardPage(_documentService));
            
            // Cảnh báo VB quá hạn khi khởi động — Theo Điều 24, NĐ 30/2020
            CheckOverdueOnStartup();
            
            Console.WriteLine("✅ MainWindow initialized successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ ERROR in MainWindow constructor: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                Console.WriteLine($"Inner stack trace: {ex.InnerException.StackTrace}");
            }
            
            MessageBox.Show(
                $"Lỗi khởi tạo ứng dụng:\n\n{ex.Message}\n\n" +
                $"Chi tiết: {ex.InnerException?.Message ?? "Không có thông tin thêm"}\n\n" +
                $"Vui lòng kiểm tra console log để biết thêm chi tiết.",
                "Lỗi khởi động",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
            
            throw; // Re-throw to show in global exception handler
        }
    }
    
    /// <summary>
    /// Kiểm tra và cảnh báo VB quá hạn khi khởi động — Điều 24, NĐ 30/2020
    /// </summary>
    private void CheckOverdueOnStartup()
    {
        try
        {
            var overdueList = _documentService.GetOverdueDocuments();
            if (overdueList.Count > 0)
            {
                var details = string.Join("\n", overdueList
                    .OrderBy(d => d.DueDate)
                    .Take(5)
                    .Select(d => $"  • {d.Number} — {d.Title} (hạn: {d.DueDate:dd/MM/yyyy})"));
                
                var moreText = overdueList.Count > 5 ? $"\n  ... và {overdueList.Count - 5} VB khác" : "";
                
                MessageBox.Show(
                    $"⚠️ CÓ {overdueList.Count} VĂN BẢN ĐẾN ĐÃ QUÁ HẠN XỬ LÝ!\n\n" +
                    $"{details}{moreText}\n\n" +
                    "Vui lòng xử lý hoặc cập nhật hạn giải quyết.",
                    "Cảnh báo quá hạn — Điều 24, NĐ 30/2020",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ CheckOverdueOnStartup error: {ex.Message}");
        }
    }
    
    private void InitializeDefaultData()
    {
        try
        {
            Console.WriteLine("🔧 Initializing default data...");
            var seeder = new TemplateSeeder(_documentService);
            seeder.SeedDefaultTemplates();
            
            // Kiểm tra số lượng templates sau khi seed
            var templateCount = _documentService.GetAllTemplates().Count;
            Console.WriteLine($"✅ Template count after seeding: {templateCount}");
            
            if (templateCount == 0)
            {
                MessageBox.Show("⚠️ Không tìm thấy template nào! Vui lòng kiểm tra console log.", 
                    "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            
            // Không auto-seed demo meetings — user tự tạo khi cần
            // Nút "Tạo dữ liệu demo" vẫn có sẵn trong trang Lịch họp
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Warning: Could not seed templates: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            MessageBox.Show($"Lỗi khi khởi tạo templates:\n{ex.Message}", 
                "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    private void LoadStatistics()
    {
        var total = _documentService.GetTotalDocuments();
        txtTotalDocs.Text = $"Tổng: {total} văn bản";
        
        var thisMonth = _documentService.GetDocumentsByDateRange(
            new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1),
            DateTime.Now
        ).Count;
        txtThisMonth.Text = $"Tháng này: {thisMonth}";
        
        var thisYear = _documentService.GetDocumentsByYear(DateTime.Now.Year).Count;
        txtThisYear.Text = $"Năm nay: {thisYear}";
    }
    
    private void NavigateToDashboard(object sender, RoutedEventArgs e)
    {
        WelcomeScreen.Visibility = Visibility.Collapsed;
        MainFrame.Navigate(new Views.DashboardPage(_documentService));
    }
    
    private void NavigateToDocuments(object sender, RoutedEventArgs e)
    {
        try
        {
            WelcomeScreen.Visibility = Visibility.Collapsed;
            MainFrame.Navigate(new Views.DocumentListPage(_documentService));
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi: {ex.Message}\n\nChi tiết: {ex.StackTrace}", "Lỗi", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Cập nhật trạng thái (enable/disable) của các nút AI trên sidebar.
    /// Gọi khi khởi tạo và sau khi settings thay đổi.
    /// </summary>
    private void UpdateAiSidebarState()
    {
        var aiReady = AppSettingsService.IsAiReady();
        var opacity = aiReady ? 1.0 : 0.5;

        // Dim nhóm header AI
        if (txtGroupAI != null)
            txtGroupAI.Opacity = aiReady ? 0.85 : 0.4;

        // Dim/enable từng button AI trên sidebar
        var aiButtons = new[] { btnAI, btnAIReport, btnAIScan, btnAIReview, btnAIAdvisory, btnAISummary };
        foreach (var btn in aiButtons)
        {
            if (btn != null)
                btn.Opacity = opacity;
        }
    }
    
    private void NavigateToAI(object sender, RoutedEventArgs e)
    {
        if (!AiPromoHelper.CheckOrShowPromo(this)) return;
        WelcomeScreen.Visibility = Visibility.Collapsed;
        MainFrame.Navigate(new Views.AIGeneratorPage(_documentService));
    }

    private void OpenAIReport_Click(object sender, RoutedEventArgs e)
    {
        if (!AiPromoHelper.CheckOrShowPromo(this)) return;
        var dialog = new Views.PeriodicReportDialog(_documentService);
        dialog.Owner = this;
        dialog.ShowDialog();
    }

    private void OpenAIScan_Click(object sender, RoutedEventArgs e)
    {
        if (!AiPromoHelper.CheckOrShowPromo(this)) return;
        var dialog = new Views.ScanImportDialog(_documentService);
        dialog.Owner = this;
        if (dialog.ShowDialog() == true)
        {
            // Refresh nếu đang ở trang Documents
            if (MainFrame.Content is Views.DocumentListPage)
                NavigateToDocuments(sender, e);
        }
    }

    private void OpenAIReview_Click(object sender, RoutedEventArgs e)
    {
        if (!AiPromoHelper.CheckOrShowPromo(this)) return;
        var doc = PickDocumentForAI("AI Kiểm tra văn bản");
        if (doc == null) return;

        var typeName = doc.Type.GetDisplayName();
        var dialog = new Views.DocumentReviewDialog(doc.Content ?? "", typeName, doc.Title, doc.Issuer);
        dialog.Owner = this;
        if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.AppliedContent))
        {
            doc.Content = dialog.AppliedContent;
            _documentService.UpdateDocument(doc);
            MessageBox.Show("✅ Đã áp dụng nội dung đã sửa vào văn bản!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void OpenAIAdvisory_Click(object sender, RoutedEventArgs e)
    {
        if (!AiPromoHelper.CheckOrShowPromo(this)) return;
        var doc = PickDocumentForAI("AI Tham mưu xử lý");
        if (doc == null) return;

        var contentToAnalyze = GetAnalyzableContent(doc);
        if (contentToAnalyze == null) return;

        var typeName = doc.Type.GetDisplayName();

        // Tạo context đầy đủ từ Document metadata
        var advisoryContext = DocumentAdvisoryContext.FromDocument(doc);

        // Load tóm tắt VB liên quan (nếu có RelatedDocumentIds)
        if (doc.RelatedDocumentIds?.Length > 0)
        {
            var relatedSummaries = new System.Collections.Generic.List<string>();
            foreach (var relId in doc.RelatedDocumentIds.Take(5))
            {
                var relDoc = _documentService.GetDocument(relId);
                if (relDoc != null)
                {
                    relatedSummaries.Add($"- [{relDoc.Type.GetDisplayName()}] {relDoc.Number} — {relDoc.Title} ({relDoc.Issuer}, {relDoc.IssueDate:dd/MM/yyyy})");
                }
            }
            if (relatedSummaries.Count > 0)
                advisoryContext.RelatedDocumentsSummary = string.Join("\n", relatedSummaries);
        }

        var dialog = new Views.DocumentAdvisoryDialog(contentToAnalyze, typeName, doc.Title, doc.Issuer, advisoryContext);
        dialog.Owner = this;
        dialog.ShowDialog();
    }

    private void OpenAISummary_Click(object sender, RoutedEventArgs e)
    {
        if (!AiPromoHelper.CheckOrShowPromo(this)) return;
        var doc = PickDocumentForAI("AI Tóm tắt văn bản");
        if (doc == null) return;

        var contentToAnalyze = GetAnalyzableContent(doc);
        if (contentToAnalyze == null) return;

        var typeName = doc.Type.GetDisplayName();
        var dialog = new Views.DocumentSummaryDialog(contentToAnalyze, typeName, doc.Title, doc.Issuer);
        dialog.Owner = this;
        dialog.ShowDialog();
    }

    /// <summary>Hiện dialog chọn văn bản cho tính năng AI</summary>
    private AIVanBan.Core.Models.Document? PickDocumentForAI(string featureName)
    {
        var allDocs = _documentService.GetAllDocuments();
        if (allDocs == null || allDocs.Count == 0)
        {
            MessageBox.Show("Chưa có văn bản nào trong hệ thống.\nHãy thêm văn bản trước khi sử dụng tính năng AI.",
                featureName, MessageBoxButton.OK, MessageBoxImage.Information);
            return null;
        }

        // Tạo danh sách để chọn
        var items = allDocs.OrderByDescending(d => d.IssueDate)
            .Select(d => new { Doc = d, Display = $"{d.Number} — {d.Title} ({d.IssueDate:dd/MM/yyyy})" })
            .ToList();

        var picker = new Window
        {
            Title = $"{featureName} — Chọn văn bản",
            Width = 700,
            Height = 500,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = this
        };

        var grid = new System.Windows.Controls.Grid();
        grid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = System.Windows.GridLength.Auto });
        grid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = new System.Windows.GridLength(1, System.Windows.GridUnitType.Star) });
        grid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = System.Windows.GridLength.Auto });

        var header = new System.Windows.Controls.TextBlock
        {
            Text = $"Chọn văn bản để {featureName}:",
            FontSize = 15,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(15, 15, 15, 10)
        };
        System.Windows.Controls.Grid.SetRow(header, 0);

        var listBox = new System.Windows.Controls.ListBox { Margin = new Thickness(15, 0, 15, 0) };
        foreach (var item in items)
            listBox.Items.Add(new System.Windows.Controls.ListBoxItem { Content = item.Display, Tag = item.Doc });

        System.Windows.Controls.Grid.SetRow(listBox, 1);

        var btnPanel = new System.Windows.Controls.StackPanel
        {
            Orientation = System.Windows.Controls.Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(15)
        };
        var btnOk = new System.Windows.Controls.Button { Content = "Chọn", Width = 100, IsDefault = true, Margin = new Thickness(0, 0, 10, 0) };
        var btnCancel = new System.Windows.Controls.Button { Content = "Hủy", Width = 100, IsCancel = true };
        btnOk.Click += (s, ev) => { if (listBox.SelectedItem != null) picker.DialogResult = true; };
        btnCancel.Click += (s, ev) => picker.DialogResult = false;
        listBox.MouseDoubleClick += (s, ev) => { if (listBox.SelectedItem != null) picker.DialogResult = true; };
        btnPanel.Children.Add(btnOk);
        btnPanel.Children.Add(btnCancel);
        System.Windows.Controls.Grid.SetRow(btnPanel, 2);

        grid.Children.Add(header);
        grid.Children.Add(listBox);
        grid.Children.Add(btnPanel);
        picker.Content = grid;

        if (picker.ShowDialog() == true && listBox.SelectedItem is System.Windows.Controls.ListBoxItem selectedItem
            && selectedItem.Tag is AIVanBan.Core.Models.Document selectedDoc)
        {
            return selectedDoc;
        }
        return null;
    }

    /// <summary>Lấy nội dung phân tích được từ Document</summary>
    private string? GetAnalyzableContent(AIVanBan.Core.Models.Document doc)
    {
        var content = doc.Content;
        if (!string.IsNullOrWhiteSpace(content) && content.Length >= 10)
            return content;

        var fallbackParts = new System.Collections.Generic.List<string>();
        if (!string.IsNullOrWhiteSpace(doc.Title)) fallbackParts.Add($"Tiêu đề: {doc.Title}");
        if (!string.IsNullOrWhiteSpace(doc.Subject)) fallbackParts.Add($"Trích yếu: {doc.Subject}");
        if (!string.IsNullOrWhiteSpace(doc.Issuer)) fallbackParts.Add($"Cơ quan ban hành: {doc.Issuer}");
        if (!string.IsNullOrWhiteSpace(doc.Number)) fallbackParts.Add($"Số hiệu: {doc.Number}");

        if (fallbackParts.Count == 0)
        {
            MessageBox.Show("Văn bản chưa có nội dung để phân tích.\nVui lòng nhập nội dung trước.",
                "Thiếu nội dung", MessageBoxButton.OK, MessageBoxImage.Warning);
            return null;
        }
        return string.Join("\n", fallbackParts);
    }

    private void NavigateToTemplates(object sender, RoutedEventArgs e)
    {
        WelcomeScreen.Visibility = Visibility.Collapsed;
        MainFrame.Navigate(new Views.TemplateManagementPage(_documentService));
    }

    // Theo Điều 1, NĐ 30/2020/NĐ-CP — Tra cứu pháp quy văn thư
    private void NavigateToLegalReference(object sender, RoutedEventArgs e)
    {
        WelcomeScreen.Visibility = Visibility.Collapsed;
        MainFrame.Navigate(new Views.LegalReferencePage());
    }

    private void NavigateToStatistics(object sender, RoutedEventArgs e)
    {
        WelcomeScreen.Visibility = Visibility.Collapsed;
        MainFrame.Navigate(new Views.StatisticsPage(_documentService));
    }
    
    private void NavigateToPhotos(object? sender, RoutedEventArgs? e)
    {
        WelcomeScreen.Visibility = Visibility.Collapsed;
        MainFrame.Navigate(new Views.PhotoAlbumPageSimple());
    }

    private void NavigateToMeetings(object sender, RoutedEventArgs e)
    {
        WelcomeScreen.Visibility = Visibility.Collapsed;
        MainFrame.Navigate(new Views.MeetingListPage(_documentService));
    }

    private void NavigateToCalendar(object sender, RoutedEventArgs e)
    {
        WelcomeScreen.Visibility = Visibility.Collapsed;
        MainFrame.Navigate(new Views.CalendarPage(_documentService));
    }

    private void NavigateToBackup(object sender, RoutedEventArgs e)
    {
        WelcomeScreen.Visibility = Visibility.Collapsed;
        MainFrame.Navigate(new Views.BackupRestorePage());
    }

    private void Help_Click(object sender, RoutedEventArgs e)
    {
        OpenContextSensitiveHelp();
    }

    private void HelpCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
    {
        OpenContextSensitiveHelp();
    }

    /// <summary>Open Help page and navigate to section relevant to current page</summary>
    private void OpenContextSensitiveHelp()
    {
        WelcomeScreen.Visibility = Visibility.Collapsed;

        // Map current page to help section
        var sectionName = MainFrame.Content switch
        {
            Views.DashboardPage => "secInterface",
            Views.DocumentListPage => "secDocManage",
            Views.TemplateManagementPage => "secTemplate",
            Views.StatisticsPage => "secDocManage",
            Views.AIGeneratorPage => "secAICompose",
            Views.PhotoAlbumPageSimple => "secAlbum",
            Views.MeetingListPage => "secMeeting",
            Views.BackupRestorePage => "secBackup",
            Views.HelpPage => (string?)null, // Already on help
            _ => null
        };

        if (MainFrame.Content is Views.HelpPage)
            return; // Already viewing help

        if (sectionName != null)
            MainFrame.Navigate(new Views.HelpPage(sectionName));
        else
            MainFrame.Navigate(new Views.HelpPage());
    }

    private void NavigateToAdmin(object sender, RoutedEventArgs e)
    {
        WelcomeScreen.Visibility = Visibility.Collapsed;
        MainFrame.Navigate(new Views.AdminDashboardPage());
    }

    /// <summary>
    /// Kiểm tra lần chạy đầu: nếu chưa có cấu hình cơ quan VÀ chưa có album → hiển thị Unified Wizard.
    /// Wizard tạo đồng thời: thư mục tài liệu + album ảnh + cấu hình CQ.
    /// </summary>
    private void CheckFirstRunSetup()
    {
        try
        {
            var orgConfig = _documentService.GetOrganizationConfig();
            var hasOrgConfig = !string.IsNullOrEmpty(orgConfig.Name);
            var hasAlbumTemplate = _albumService.GetActiveTemplate() != null;
            var hasFolders = _documentService.GetAllFolders().Count >= 5;
            
            // Chỉ hiện wizard nếu CHƯA setup gì cả
            if (!hasOrgConfig && !hasAlbumTemplate && !hasFolders)
            {
                Console.WriteLine("🏛️ First run detected — showing Unified Setup Wizard...");
                
                var wizard = new Views.UnifiedSetupWizard(_documentService, _albumService)
                {
                    Owner = this
                };
                wizard.ShowDialog();
            }
            else
            {
                Console.WriteLine($"✅ Setup already done (org={hasOrgConfig}, album={hasAlbumTemplate}, folders={hasFolders})");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Warning: First-run setup check failed: {ex.Message}");
        }
    }

    private void SetupAlbumStructure(object? sender, RoutedEventArgs? e)
    {
        try
        {
            var dialog = new AlbumStructureSetupDialog(_albumService)
            {
                Owner = this
            };
            if (dialog.ShowDialog() == true)
            {
                MessageBox.Show(
                    "✅ Đã thiết lập cấu trúc Album thành công!\n\n" +
                    "Bạn có thể bắt đầu thêm ảnh vào các album theo nghiệp vụ.",
                    "Thành công",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                
                // Reload photos page if currently viewing
                if (MainFrame.Content is PhotoAlbumPageSimple)
                {
                    NavigateToPhotos(null, null);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"❌ Lỗi khi mở dialog thiết lập:\n{ex.Message}",
                "Lỗi",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
    
    private void ToggleSidebar_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _isSidebarCollapsed = !_isSidebarCollapsed;
            
            if (_isSidebarCollapsed)
            {
                // Collapse to 60px (icon only)
                sidebarColumn.Width = new GridLength(60);
                iconToggle.Kind = MaterialDesignThemes.Wpf.PackIconKind.ChevronRight;
                btnToggleSidebar.ToolTip = "Mở rộng menu";
                
                // Hide text labels
                txtDashboard.Visibility = Visibility.Collapsed;
                txtDocuments.Visibility = Visibility.Collapsed;
                txtTemplates.Visibility = Visibility.Collapsed;
                txtLegalRef.Visibility = Visibility.Collapsed;
                txtStatistics.Visibility = Visibility.Collapsed;
                txtPhotos.Visibility = Visibility.Collapsed;
                txtMeetings.Visibility = Visibility.Collapsed;
                txtCalendar.Visibility = Visibility.Collapsed;
                txtAI.Visibility = Visibility.Collapsed;
                txtAIReport.Visibility = Visibility.Collapsed;
                txtAIScan.Visibility = Visibility.Collapsed;
                txtAIReview.Visibility = Visibility.Collapsed;
                txtAIAdvisory.Visibility = Visibility.Collapsed;
                txtAISummary.Visibility = Visibility.Collapsed;
                txtAlbumSetup.Visibility = Visibility.Collapsed;
                txtBackup.Visibility = Visibility.Collapsed;
                txtHelp.Visibility = Visibility.Collapsed;
                
                // Hide group headers & stats
                txtGroupDocuments.Visibility = Visibility.Collapsed;
                txtGroupWork.Visibility = Visibility.Collapsed;
                txtGroupAI.Visibility = Visibility.Collapsed;
                txtGroupSystem.Visibility = Visibility.Collapsed;
                separatorStats.Visibility = Visibility.Collapsed;
                statsPanel.Visibility = Visibility.Collapsed;
                
                // Center button content
                var allButtons = new[] { btnDashboard, btnDocuments, btnTemplates,
                    btnLegalRef, btnStatistics, btnPhotos, btnMeetings, btnAI, btnAIReport, btnAIScan, btnAIReview,
                    btnAIAdvisory, btnAISummary, btnAlbumSetup, btnBackup, btnHelp };
                foreach (var btn in allButtons)
                {
                    btn.HorizontalContentAlignment = HorizontalAlignment.Center;
                    btn.Padding = new Thickness(0);
                }
            }
            else
            {
                // Expand to 240px (full menu)
                sidebarColumn.Width = new GridLength(240);
                iconToggle.Kind = MaterialDesignThemes.Wpf.PackIconKind.ChevronLeft;
                btnToggleSidebar.ToolTip = "Thu gọn menu";
                
                // Show text labels
                txtDashboard.Visibility = Visibility.Visible;
                txtDocuments.Visibility = Visibility.Visible;
                txtTemplates.Visibility = Visibility.Visible;
                txtLegalRef.Visibility = Visibility.Visible;
                txtStatistics.Visibility = Visibility.Visible;
                txtPhotos.Visibility = Visibility.Visible;
                txtMeetings.Visibility = Visibility.Visible;
                txtCalendar.Visibility = Visibility.Visible;
                txtAI.Visibility = Visibility.Visible;
                txtAIReport.Visibility = Visibility.Visible;
                txtAIScan.Visibility = Visibility.Visible;
                txtAIReview.Visibility = Visibility.Visible;
                txtAIAdvisory.Visibility = Visibility.Visible;
                txtAISummary.Visibility = Visibility.Visible;
                txtAlbumSetup.Visibility = Visibility.Visible;
                txtBackup.Visibility = Visibility.Visible;
                txtHelp.Visibility = Visibility.Visible;
                
                // Show group headers & stats
                txtGroupDocuments.Visibility = Visibility.Visible;
                txtGroupWork.Visibility = Visibility.Visible;
                txtGroupAI.Visibility = Visibility.Visible;
                txtGroupSystem.Visibility = Visibility.Visible;
                separatorStats.Visibility = Visibility.Visible;
                statsPanel.Visibility = Visibility.Visible;
                
                // Restore button alignment
                var allButtons = new[] { btnDashboard, btnDocuments, btnTemplates,
                    btnLegalRef, btnStatistics, btnPhotos, btnMeetings, btnAI, btnAIReport, btnAIScan, btnAIReview,
                    btnAIAdvisory, btnAISummary, btnAlbumSetup, btnBackup, btnHelp };
                foreach (var btn in allButtons)
                {
                    btn.HorizontalContentAlignment = HorizontalAlignment.Left;
                    btn.Padding = new Thickness(20, 0, 0, 0);
                }
                btnDashboard.Padding = new Thickness(12, 0, 0, 0);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi toggle sidebar: {ex.Message}", "Lỗi", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
        
    protected override void OnClosed(EventArgs e)
    {
        _documentService?.Dispose();
        _albumService?.Dispose();
        base.OnClosed(e);
    }
    
    private void CheckForUpdates_Click(object sender, RoutedEventArgs e)
    {
        AppUpdateService.CheckForUpdateManual();
    }
    
    private void About_Click(object sender, RoutedEventArgs e)
    {
        var aboutDialog = new AboutDialog
        {
            Owner = this
        };
        aboutDialog.ShowDialog();
    }

    private void BrandUrl_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("https://giakiemso.com") { UseShellExecute = true });
        }
        catch { }
    }

    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        var settingsDialog = new ApiSettingsDialog
        {
            Owner = this
        };
        if (settingsDialog.ShowDialog() == true)
        {
            // Reload status bar và trạng thái AI sidebar sau khi settings thay đổi
            LoadApiStatusBar();
            UpdateAiSidebarState();
        }
    }

    private void QuickLogin_Click(object sender, RoutedEventArgs e)
    {
        var settings = AppSettingsService.Load();
        if (settings.UseVanBanPlusApi && !string.IsNullOrEmpty(settings.VanBanPlusApiKey) 
            && !string.IsNullOrEmpty(settings.UserEmail))
        {
            // Đã đăng nhập → mở UserProfile
            var profileDialog = new UserProfileDialog { Owner = this };
            profileDialog.ShowDialog();
            LoadApiStatusBar();
        }
        else
        {
            // Chưa đăng nhập → mở Login dialog
            var loginDialog = new LoginRegisterDialog { Owner = this };
            if (loginDialog.ShowDialog() == true)
            {
                LoadApiStatusBar();
            }
        }
    }

    private void Logout_Click(object sender, RoutedEventArgs e)
    {
        var confirm = MessageBox.Show(
            "Đăng xuất khỏi tài khoản VanBanPlus?\n\nMã kích hoạt sẽ được giữ lại, chỉ xóa thông tin đăng nhập.",
            "Đăng xuất", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (confirm != MessageBoxResult.Yes) return;

        var settings = AppSettingsService.Load();
        settings.UserEmail = "";
        settings.UserFullName = "";
        settings.UserPlan = "";
        settings.UserRole = "user";
        settings.VanBanPlusApiKey = "";
        AppSettingsService.Save(settings);

        // Ẩn Admin button
        btnAdmin.Visibility = Visibility.Collapsed;

        // Reload status bar
        LoadApiStatusBar();

        // Quay về Dashboard
        WelcomeScreen.Visibility = Visibility.Collapsed;
        MainFrame.Navigate(new Views.DashboardPage(_documentService));
    }

    private void LoadApiStatusBar()
    {
        try
        {
            var settings = AppSettingsService.Load();
            
            if (settings.UseVanBanPlusApi && !string.IsNullOrEmpty(settings.VanBanPlusApiKey))
            {
                // VanBanPlus mode
                iconApiStatus.Kind = MaterialDesignThemes.Wpf.PackIconKind.CloudCheck;
                iconApiStatus.Foreground = System.Windows.Media.Brushes.Green;
                txtApiMode.Text = "☁️ VanBanPlus API";
                
                // Show user info from cache
                if (!string.IsNullOrEmpty(settings.UserEmail))
                {
                    txtStatusUser.Text = $"{settings.UserFullName} ({settings.UserPlan})";
                    btnLoginQuick.Content = "👤 " + settings.UserFullName;
                    btnLoginQuick.Visibility = Visibility.Visible;
                    btnLogout.Visibility = Visibility.Visible;
                }
                else
                {
                    txtStatusUser.Text = "Chưa đăng nhập";
                    btnLoginQuick.Content = "🔑 Đăng nhập";
                    btnLoginQuick.Visibility = Visibility.Visible;
                    btnLogout.Visibility = Visibility.Collapsed;
                }

                // Show admin button if user is admin (check via API in background)
                _ = CheckAdminRoleAsync(settings);

                // Fetch usage in background
                _ = FetchUsageAsync(settings);
            }
            else if (!string.IsNullOrEmpty(settings.GeminiApiKey))
            {
                // AI direct mode (dev/maintenance only)
                iconApiStatus.Kind = MaterialDesignThemes.Wpf.PackIconKind.Wrench;
                iconApiStatus.Foreground = System.Windows.Media.Brushes.Orange;
                txtApiMode.Text = "🔧 Bảo trì";
                txtStatusUser.Text = "Đã kích hoạt";
                txtUsageInfo.Text = "";
                btnLoginQuick.Content = "🔑 Đăng nhập";
                btnLoginQuick.Visibility = Visibility.Visible;
                btnLogout.Visibility = Visibility.Collapsed;
            }
            else
            {
                // No config
                iconApiStatus.Kind = MaterialDesignThemes.Wpf.PackIconKind.CloudOff;
                iconApiStatus.Foreground = System.Windows.Media.Brushes.Red;
                txtApiMode.Text = "⚠️ Chưa cấu hình";
                txtStatusUser.Text = "";
                txtUsageInfo.Text = "";
                btnLoginQuick.Content = "🔑 Đăng nhập";
                btnLoginQuick.Visibility = Visibility.Visible;
                btnLogout.Visibility = Visibility.Collapsed;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ LoadApiStatusBar error: {ex.Message}");
        }
    }

    private async Task FetchUsageAsync(AppSettings settings)
    {
        try
        {
            using var http = new HttpClient();
            http.Timeout = TimeSpan.FromSeconds(10);
            http.DefaultRequestHeaders.Add("X-API-Key", settings.VanBanPlusApiKey);
            if (!string.IsNullOrEmpty(settings.VercelBypassToken))
                http.DefaultRequestHeaders.Add("x-vercel-protection-bypass", settings.VercelBypassToken);

            var url = settings.VanBanPlusApiUrl.TrimEnd('/');
            
            // Fetch profile + cache user info
            var meResp = await http.GetAsync($"{url}/api/auth/me");
            if (meResp.IsSuccessStatusCode)
            {
                var meResult = await meResp.Content.ReadFromJsonAsync<ApiResponse<UserProfile>>();
                if (meResult?.Data != null)
                {
                    settings.UserEmail = meResult.Data.Email;
                    settings.UserFullName = meResult.Data.FullName;
                    settings.UserPlan = meResult.Data.Plan;
                    AppSettingsService.Save(settings);
                    
                    Dispatcher.Invoke(() =>
                    {
                        txtStatusUser.Text = $"{meResult.Data.FullName} ({meResult.Data.Plan})";
                    });
                }
            }

            // Fetch usage
            var usageResp = await http.GetAsync($"{url}/api/usage");
            if (usageResp.IsSuccessStatusCode)
            {
                var usageResult = await usageResp.Content.ReadFromJsonAsync<ApiResponse<UsageSummary>>();
                if (usageResult?.Data != null)
                {
                    var u = usageResult.Data;
                    Dispatcher.Invoke(() =>
                    {
                        txtUsageInfo.Text = $"📊 Tháng này: {u.TotalRequests} requests | {u.TotalTokens:N0} tokens";
                    });
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ FetchUsageAsync error: {ex.Message}");
        }
    }

    private async Task CheckAdminRoleAsync(AppSettings settings)
    {
        try
        {
            using var http = new HttpClient();
            http.Timeout = TimeSpan.FromSeconds(10);
            http.DefaultRequestHeaders.Add("X-API-Key", settings.VanBanPlusApiKey);
            if (!string.IsNullOrEmpty(settings.VercelBypassToken))
                http.DefaultRequestHeaders.Add("x-vercel-protection-bypass", settings.VercelBypassToken);

            var url = settings.VanBanPlusApiUrl.TrimEnd('/');
            var resp = await http.GetAsync($"{url}/api/auth/me");
            if (resp.IsSuccessStatusCode)
            {
                var result = await resp.Content.ReadFromJsonAsync<ApiResponse<UserProfile>>();
                if (result?.Data != null)
                {
                    settings.UserRole = result.Data.Role;
                    AppSettingsService.Save(settings);

                    Dispatcher.Invoke(() =>
                    {
                        btnAdmin.Visibility = result.Data.Role == "admin" 
                            ? Visibility.Visible : Visibility.Collapsed;
                    });
                }
            }
        }
        catch { /* ignore */ }
    }

    #region API DTOs
    private class ApiResponse<T>
    {
        [JsonPropertyName("success")] public bool Success { get; set; }
        [JsonPropertyName("data")] public T? Data { get; set; }
    }
    private class UserProfile
    {
        [JsonPropertyName("email")] public string Email { get; set; } = "";
        [JsonPropertyName("fullName")] public string FullName { get; set; } = "";
        [JsonPropertyName("plan")] public string Plan { get; set; } = "";
        [JsonPropertyName("role")] public string Role { get; set; } = "user";
    }
    private class UsageSummary
    {
        [JsonPropertyName("requestsUsed")] public int TotalRequests { get; set; }
        [JsonPropertyName("tokensUsed")] public long TotalTokens { get; set; }
        [JsonPropertyName("estimatedCostThisMonth")] public double TotalCost { get; set; }
    }
    #endregion
}
