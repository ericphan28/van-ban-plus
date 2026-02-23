using System;
using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using AIVanBan.Core.Data;
using AIVanBan.Desktop.Services;

namespace AIVanBan.Desktop;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        // Global exception handlers
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        DispatcherUnhandledException += App_DispatcherUnhandledException;
        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        
        Console.WriteLine("✅ Global exception handlers registered");
        
        // Khởi tạo shared database trước tất cả services
        DatabaseFactory.GetDatabase();
        
        // Auto-update: check for updates on startup
        AppUpdateService.Initialize();
        AppUpdateService.CheckForUpdateSilent();
        
        // Auto-backup: sao lưu tự động khi mở app
        try
        {
            var backupService = new AIVanBan.Core.Services.BackupService();
            var backupResult = backupService.AutoBackup();
            if (backupResult.Success && !backupResult.Skipped)
                Console.WriteLine($"✅ Auto-backup: {backupResult.FilePath}");
            else if (backupResult.Skipped)
                Console.WriteLine("✅ Auto-backup: Skipped (recent backup exists)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Auto-backup failed: {ex.Message}");
        }
    }
    
    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var exception = e.ExceptionObject as Exception;
        LogAndShowError("FATAL ERROR (UnhandledException)", exception);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        DatabaseFactory.Shutdown();
        base.OnExit(e);
    }
    
    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        LogAndShowError("UI ERROR (DispatcherUnhandledException)", e.Exception);
        e.Handled = true; // Prevent crash
    }
    
    private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        LogAndShowError("TASK ERROR (UnobservedTaskException)", e.Exception);
        e.SetObserved(); // Prevent crash
    }
    
    private void LogAndShowError(string title, Exception? exception)
    {
        if (exception == null) return;
        
        // Skip XAML parsing errors to prevent infinite loop (but log details)
        if (exception is System.Windows.Markup.XamlParseException xamlEx ||
            exception.GetType().Name.Contains("Xaml") ||
            exception.Message.Contains("TypeConverterMarkupExtension"))
        {
            var lineInfo = "";
            if (exception is System.Windows.Markup.XamlParseException parseEx)
            {
                lineInfo = $" Line: {parseEx.LineNumber}, Position: {parseEx.LinePosition}, BaseUri: {parseEx.BaseUri}";
            }

            Console.WriteLine($"\n⚠️ XAML ERROR (skipped dialog): {exception.Message}{lineInfo}");
            if (exception.InnerException != null)
            {
                Console.WriteLine($"Inner: {exception.InnerException.GetType().Name}: {exception.InnerException.Message}");
            }
            return;
        }
        
        var errorMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {title}\n" +
                          $"Message: {exception.Message}\n" +
                          $"Type: {exception.GetType().Name}\n" +
                          $"StackTrace: {exception.StackTrace}";
        
        // Log to console
        Console.WriteLine("\n" + new string('=', 80));
        Console.WriteLine("❌ ERROR CAUGHT:");
        Console.WriteLine(errorMessage);
        Console.WriteLine(new string('=', 80) + "\n");
        
        // Log to file
        try
        {
            var logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            Directory.CreateDirectory(logDir);
            var logFile = Path.Combine(logDir, $"error_{DateTime.Now:yyyyMMdd}.log");
            File.AppendAllText(logFile, errorMessage + "\n\n");
        }
        catch { /* Ignore logging errors */ }
        
        // Show to user
        var result = MessageBox.Show(
            $"❌ LỖI CHƯƠNG TRÌNH\n\n" +
            $"Message: {exception.Message}\n\n" +
            $"Type: {exception.GetType().Name}\n\n" +
            $"Bạn có muốn xem chi tiết lỗi không?",
            "Lỗi",
            MessageBoxButton.YesNo,
            MessageBoxImage.Error);
        
        if (result == MessageBoxResult.Yes)
        {
            MessageBox.Show(
                errorMessage,
                "Chi tiết lỗi (có thể copy)",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}

