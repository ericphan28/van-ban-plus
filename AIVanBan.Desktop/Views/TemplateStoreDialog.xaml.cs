using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AIVanBan.Core.Services;

namespace AIVanBan.Desktop.Views;

public partial class TemplateStoreDialog : Window
{
    private readonly TemplateStoreService _storeService;
    private readonly DocumentService _documentService;
    private List<StoreTemplateViewModel> _allItems = new();
    
    /// <summary>
    /// Số template đã tải/cập nhật trong session này
    /// </summary>
    public int DownloadedCount { get; private set; }

    public TemplateStoreDialog(DocumentService documentService)
    {
        InitializeComponent();
        _documentService = documentService;
        _storeService = new TemplateStoreService(documentService);
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadStoreAsync();
    }

    private async System.Threading.Tasks.Task LoadStoreAsync()
    {
        ShowLoading(true);
        
        try
        {
            _allItems = await _storeService.FetchStoreTemplatesAsync();
            
            // Populate category filter
            var categories = _allItems.Select(i => i.CategoryDisplay).Distinct().OrderBy(c => c).ToList();
            cboCategory.Items.Clear();
            cboCategory.Items.Add("Tất cả");
            foreach (var cat in categories)
                cboCategory.Items.Add(cat);
            cboCategory.SelectedIndex = 0;
            
            ApplyFilters();
            ShowResults();
            UpdateFooter();
            UpdateDownloadAllButton();
        }
        catch (Exception ex)
        {
            ShowError($"Không thể kết nối kho mẫu online.\n\n{ex.Message}\n\nVui lòng kiểm tra kết nối Internet.");
        }
    }

    private void ApplyFilters()
    {
        var filtered = _allItems.AsEnumerable();
        
        // Search
        if (!string.IsNullOrWhiteSpace(txtSearch.Text))
        {
            var keyword = txtSearch.Text.ToLower();
            filtered = filtered.Where(i =>
                i.Template.Name.ToLower().Contains(keyword) ||
                i.Template.Description.ToLower().Contains(keyword) ||
                i.Template.Category.ToLower().Contains(keyword) ||
                i.Template.Tags.Any(t => t.ToLower().Contains(keyword)));
        }
        
        // Category
        if (cboCategory.SelectedIndex > 0 && cboCategory.SelectedItem is string selectedCat)
        {
            filtered = filtered.Where(i => i.CategoryDisplay == selectedCat);
        }
        
        // Status
        if (cboStatus.SelectedIndex > 0)
        {
            var statusFilter = cboStatus.SelectedIndex switch
            {
                1 => StoreTemplateStatus.NotDownloaded,
                2 => StoreTemplateStatus.UpToDate,
                3 => StoreTemplateStatus.UpdateAvailable,
                _ => (StoreTemplateStatus?)null
            };
            if (statusFilter.HasValue)
                filtered = filtered.Where(i => i.Status == statusFilter.Value);
        }
        
        // Sort: New/Popular first, then by name
        var sorted = filtered
            .OrderByDescending(i => i.Template.IsNew)
            .ThenByDescending(i => i.Template.IsPopular)
            .ThenBy(i => i.Template.Name)
            .ToList();
        
        lstTemplates.ItemsSource = sorted;
    }
    
    private void Search_KeyUp(object sender, KeyEventArgs e)
    {
        ApplyFilters();
    }
    
    private void Filter_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (_allItems.Count > 0)
            ApplyFilters();
    }

    private void DownloadOne_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string storeId)
        {
            var item = _allItems.FirstOrDefault(i => i.Template.StoreId == storeId);
            if (item == null || item.Status == StoreTemplateStatus.UpToDate) return;
            
            try
            {
                var result = _storeService.DownloadTemplate(item.Template);
                
                // Update status in memory
                item.Status = StoreTemplateStatus.UpToDate;
                item.LocalVersion = item.Template.Version;
                DownloadedCount++;
                
                // Refresh UI
                ApplyFilters();
                UpdateFooter();
                UpdateDownloadAllButton();
                
                var action = item.LocalVersion > 0 ? "cập nhật" : "tải về";
                MessageBox.Show(
                    $"✅ Đã {action} mẫu \"{result.Name}\" thành công!",
                    "Thành công",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"❌ Lỗi khi tải mẫu:\n{ex.Message}",
                    "Lỗi",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }

    private void DownloadAll_Click(object sender, RoutedEventArgs e)
    {
        var newCount = _allItems.Count(i => i.Status != StoreTemplateStatus.UpToDate);
        if (newCount == 0)
        {
            MessageBox.Show("✅ Tất cả mẫu đã được tải về và cập nhật mới nhất!",
                "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        
        var confirm = MessageBox.Show(
            $"Tải về {newCount} mẫu mới/cần cập nhật?\n\nCác mẫu đã có sẽ không bị ảnh hưởng.",
            "Xác nhận tải tất cả",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);
        
        if (confirm != MessageBoxResult.Yes) return;
        
        btnDownloadAll.IsEnabled = false;
        txtDownloadAllLabel.Text = "Đang tải...";
        
        try
        {
            int count = 0;
            foreach (var item in _allItems)
            {
                if (item.Status != StoreTemplateStatus.UpToDate)
                {
                    _storeService.DownloadTemplate(item.Template);
                    item.Status = StoreTemplateStatus.UpToDate;
                    item.LocalVersion = item.Template.Version;
                    count++;
                }
            }
            
            DownloadedCount += count;
            ApplyFilters();
            UpdateFooter();
            UpdateDownloadAllButton();
            
            MessageBox.Show(
                $"✅ Đã tải về {count} mẫu thành công!\n\nCác mẫu đã sẵn sàng sử dụng trong trang Quản lý mẫu.",
                "Hoàn tất",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"❌ Lỗi khi tải mẫu:\n{ex.Message}",
                "Lỗi",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            btnDownloadAll.IsEnabled = true;
            UpdateDownloadAllButton();
        }
    }

    private async void Refresh_Click(object sender, RoutedEventArgs e)
    {
        await LoadStoreAsync();
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = DownloadedCount > 0;
        Close();
    }

    // ═══ UI Helpers ═══

    private void ShowLoading(bool show)
    {
        pnlLoading.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        pnlResults.Visibility = Visibility.Collapsed;
        pnlError.Visibility = Visibility.Collapsed;
        txtLoadingMsg.Text = "🌐 Đang kết nối kho mẫu online...";
    }

    private void ShowResults()
    {
        pnlLoading.Visibility = Visibility.Collapsed;
        pnlResults.Visibility = Visibility.Visible;
        pnlError.Visibility = Visibility.Collapsed;
    }

    private void ShowError(string message)
    {
        pnlLoading.Visibility = Visibility.Collapsed;
        pnlResults.Visibility = Visibility.Collapsed;
        pnlError.Visibility = Visibility.Visible;
        txtError.Text = message;
    }

    private void UpdateFooter()
    {
        var total = _allItems.Count;
        var downloaded = _allItems.Count(i => i.Status == StoreTemplateStatus.UpToDate);
        var newCount = _allItems.Count(i => i.Status == StoreTemplateStatus.NotDownloaded);
        var updateCount = _allItems.Count(i => i.Status == StoreTemplateStatus.UpdateAvailable);
        
        var parts = new List<string>
        {
            $"📦 {total} mẫu trên store",
            $"✅ {downloaded} đã tải"
        };
        if (newCount > 0) parts.Add($"🆕 {newCount} mẫu mới");
        if (updateCount > 0) parts.Add($"⬆ {updateCount} cần cập nhật");
        
        txtFooter.Text = string.Join("  •  ", parts);
    }

    private void UpdateDownloadAllButton()
    {
        var pendingCount = _allItems.Count(i => i.Status != StoreTemplateStatus.UpToDate);
        if (pendingCount > 0)
        {
            btnDownloadAll.IsEnabled = true;
            txtDownloadAllLabel.Text = $"Tải tất cả ({pendingCount})";
        }
        else
        {
            btnDownloadAll.IsEnabled = false;
            txtDownloadAllLabel.Text = "✓ Đã đầy đủ";
        }
    }
}
