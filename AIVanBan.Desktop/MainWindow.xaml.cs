using System;
using System.Windows;
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
            
            // Check album setup on first run
            Console.WriteLine("🔧 Checking album setup...");
            CheckAlbumSetup();
            
            Console.WriteLine("🔧 Loading statistics...");
            LoadStatistics();
            
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
            
            // Seed demo meetings
            Console.WriteLine("🔧 Seeding demo meetings...");
            var meetingService = new MeetingService();
            var meetingSeeder = new MeetingSeeder(meetingService);
            meetingSeeder.SeedDemoMeetings();
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
        WelcomeScreen.Visibility = Visibility.Visible;
        MainFrame.Content = null;
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
    
    private void NavigateToAI(object sender, RoutedEventArgs e)
    {
        WelcomeScreen.Visibility = Visibility.Collapsed;
        MainFrame.Navigate(new Views.AIGeneratorPage(_documentService));
    }
    
    private void NavigateToTemplates(object sender, RoutedEventArgs e)
    {
        WelcomeScreen.Visibility = Visibility.Collapsed;
        MainFrame.Navigate(new Views.TemplateManagementPage(_documentService));
    }
    
    private void NavigateToPhotos(object sender, RoutedEventArgs e)
    {
        WelcomeScreen.Visibility = Visibility.Collapsed;
        MainFrame.Navigate(new Views.PhotoAlbumPageSimple());
    }

    private void NavigateToMeetings(object sender, RoutedEventArgs e)
    {
        WelcomeScreen.Visibility = Visibility.Collapsed;
        MainFrame.Navigate(new Views.MeetingListPage(_documentService));
    }

    private void CheckAlbumSetup()
    {
        try
        {
            var activeTemplate = _albumService.GetActiveTemplate();
            if (activeTemplate == null)
            {
                // First time - show info dialog
                var result = MessageBox.Show(
                    "🖼️ THIẾT LẬP ALBUM ẢNH\n\n" +
                    "Bạn chưa thiết lập cấu trúc Album theo nghiệp vụ cơ quan.\n\n" +
                    "Hệ thống sẽ giúp bạn:\n" +
                    "• Tạo cấu trúc folder chuẩn (12 danh mục, 70+ phân loại)\n" +
                    "• Tự động phân loại theo lĩnh vực\n" +
                    "• Gợi ý tags cho mỗi album\n\n" +
                    "Bạn có muốn thiết lập ngay bây giờ?",
                    "Thiết lập Album",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    SetupAlbumStructure(null, null);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Warning: Album setup check failed: {ex.Message}");
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
                txtAI.Visibility = Visibility.Collapsed;
                txtTemplates.Visibility = Visibility.Collapsed;
                txtPhotos.Visibility = Visibility.Collapsed;
                txtMeetings.Visibility = Visibility.Collapsed;
                txtAlbumSetup.Visibility = Visibility.Collapsed;
                separatorSettings.Visibility = Visibility.Collapsed;
                separatorStats.Visibility = Visibility.Collapsed;
                txtStatsHeader.Visibility = Visibility.Collapsed;
                statsPanel.Visibility = Visibility.Collapsed;
                
                // Center button content
                btnDashboard.HorizontalContentAlignment = HorizontalAlignment.Center;
                btnDocuments.HorizontalContentAlignment = HorizontalAlignment.Center;
                btnAI.HorizontalContentAlignment = HorizontalAlignment.Center;
                btnTemplates.HorizontalContentAlignment = HorizontalAlignment.Center;
                btnPhotos.HorizontalContentAlignment = HorizontalAlignment.Center;
                btnMeetings.HorizontalContentAlignment = HorizontalAlignment.Center;
                btnAlbumSetup.HorizontalContentAlignment = HorizontalAlignment.Center;
                btnDashboard.Padding = new Thickness(0);
                btnDocuments.Padding = new Thickness(0);
                btnAI.Padding = new Thickness(0);
                btnTemplates.Padding = new Thickness(0);
                btnAlbumSetup.Padding = new Thickness(0);
                btnPhotos.Padding = new Thickness(0);
                btnMeetings.Padding = new Thickness(0);
            }
            else
            {
                // Expand to 280px (full menu)
                sidebarColumn.Width = new GridLength(280);
                iconToggle.Kind = MaterialDesignThemes.Wpf.PackIconKind.ChevronLeft;
                btnToggleSidebar.ToolTip = "Thu gọn menu";
                
                // Show text labels
                txtDashboard.Visibility = Visibility.Visible;
                txtDocuments.Visibility = Visibility.Visible;
                txtAI.Visibility = Visibility.Visible;
                txtAlbumSetup.Visibility = Visibility.Visible;
                separatorSettings.Visibility = Visibility.Visible;
                txtTemplates.Visibility = Visibility.Visible;
                txtPhotos.Visibility = Visibility.Visible;
                txtMeetings.Visibility = Visibility.Visible;
                separatorStats.Visibility = Visibility.Visible;
                txtStatsHeader.Visibility = Visibility.Visible;
                statsPanel.Visibility = Visibility.Visible;
                
                // Restore button alignment
                btnDashboard.HorizontalContentAlignment = HorizontalAlignment.Left;
                btnDocuments.HorizontalContentAlignment = HorizontalAlignment.Left;
                btnAlbumSetup.HorizontalContentAlignment = HorizontalAlignment.Left;
                btnAI.HorizontalContentAlignment = HorizontalAlignment.Left;
                btnTemplates.HorizontalContentAlignment = HorizontalAlignment.Left;
                btnPhotos.HorizontalContentAlignment = HorizontalAlignment.Left;
                btnMeetings.HorizontalContentAlignment = HorizontalAlignment.Left;
                btnDashboard.Padding = new Thickness(20, 0, 0, 0);
                btnAlbumSetup.Padding = new Thickness(20, 0, 0, 0);
                btnDocuments.Padding = new Thickness(20, 0, 0, 0);
                btnAI.Padding = new Thickness(20, 0, 0, 0);
                btnTemplates.Padding = new Thickness(20, 0, 0, 0);
                btnPhotos.Padding = new Thickness(20, 0, 0, 0);
                btnMeetings.Padding = new Thickness(20, 0, 0, 0);
                btnAlbumSetup.Padding = new Thickness(20, 0, 0, 0);
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
}
