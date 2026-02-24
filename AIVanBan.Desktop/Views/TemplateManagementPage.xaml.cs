using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AIVanBan.Core.Models;
using AIVanBan.Core.Services;

namespace AIVanBan.Desktop.Views;

public partial class TemplateManagementPage : Page
{
    private readonly DocumentService _documentService;
    private List<DocumentTemplate> _allTemplates = new();

    public TemplateManagementPage(DocumentService documentService)
    {
        InitializeComponent();
        _documentService = documentService;
        InitializeFilters();
        LoadTemplates();
    }

    private void InitializeFilters()
    {
        cboFilterType.Items.Add("Tất cả");
        foreach (DocumentType type in Enum.GetValues(typeof(DocumentType)))
        {
            cboFilterType.Items.Add(type.GetDisplayName());
        }
        cboFilterType.SelectedIndex = 0;
    }

    private void LoadTemplates()
    {
        _allTemplates = _documentService.GetAllTemplates();
        ApplyFilters();
    }

    private void SeedDefaultTemplates_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "⚠️ CẢNH BÁO: Thao tác này sẽ XÓA TẤT CẢ các mẫu hiện tại và tạo lại 20 mẫu mặc định.\n\n" +
            "Bạn có chắc muốn tiếp tục?",
            "Xác nhận khởi tạo lại",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                // Xóa tất cả templates hiện tại
                var allTemplates = _documentService.GetAllTemplates();
                foreach (var template in allTemplates)
                {
                    _documentService.DeleteTemplate(template.Id);
                }

                // Chạy seeder để tạo 20 mẫu mới
                var seeder = new TemplateSeeder(_documentService);
                seeder.SeedDefaultTemplates();

                // Reload UI
                LoadTemplates();

                MessageBox.Show(
                    $"✅ Đã khởi tạo {_allTemplates.Count} mẫu mặc định thành công!",
                    "Thành công",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"❌ Lỗi khi khởi tạo mẫu:\n{ex.Message}",
                    "Lỗi",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }

    private void ApplyFilters()
    {
        var filtered = _allTemplates.AsEnumerable();

        // Search filter
        if (!string.IsNullOrWhiteSpace(txtSearch.Text))
        {
            var keyword = txtSearch.Text.ToLower();
            filtered = filtered.Where(t =>
                t.Name.ToLower().Contains(keyword) ||
                t.Description.ToLower().Contains(keyword));
        }

        // Type filter
        if (cboFilterType.SelectedIndex > 0 && cboFilterType.SelectedItem is string selectedTypeName)
        {
            var matchedType = Enum.GetValues(typeof(DocumentType)).Cast<DocumentType>()
                .FirstOrDefault(t => t.GetDisplayName() == selectedTypeName);
            filtered = filtered.Where(t => t.Type == matchedType);
        }

        dgTemplates.ItemsSource = filtered.ToList();
    }

    private void Search_KeyUp(object sender, KeyEventArgs e)
    {
        ApplyFilters();
    }

    private void FilterChanged(object sender, SelectionChangedEventArgs e)
    {
        ApplyFilters();
    }

    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        LoadTemplates();
    }

    private void AddTemplate_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new TemplateEditDialog(null, _documentService);
        if (dialog.ShowDialog() == true)
        {
            LoadTemplates();
        }
    }

    private void OpenStore_Click(object sender, RoutedEventArgs e)
    {
        var storeDialog = new TemplateStoreDialog(_documentService);
        storeDialog.Owner = Window.GetWindow(this);
        storeDialog.ShowDialog();
        
        // Reload nếu đã tải template mới
        if (storeDialog.DownloadedCount > 0)
        {
            LoadTemplates();
        }
    }

    private void ViewTemplate_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string id)
        {
            var template = _documentService.GetTemplateById(id);
            if (template != null)
            {
                var viewer = new TemplateViewDialog(template);
                viewer.ShowDialog();
            }
        }
    }

    private void UseTemplate_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string id)
        {
            var template = _documentService.GetTemplateById(id);
            if (template != null)
            {
                try
                {
                    // Mở AI Compose Dialog với template đã chọn sẵn
                    var dialog = new AIComposeDialog(_documentService, preSelectedTemplateId: template.Id);
                    dialog.Owner = Window.GetWindow(this);
                    
                    if (dialog.ShowDialog() == true && dialog.GeneratedDocument != null)
                    {
                        // Tăng usage count
                        template.UsageCount++;
                        _documentService.UpdateTemplate(template);
                        
                        // Lưu document vào DB
                        _documentService.AddDocument(dialog.GeneratedDocument);
                        
                        MessageBox.Show(
                            $"✅ Đã tạo và lưu văn bản:\n\n" +
                            $"📋 {dialog.GeneratedDocument.Title}\n" +
                            $"📁 Loại: {dialog.GeneratedDocument.Type.GetDisplayName()}\n" +
                            $"🏢 Cơ quan: {dialog.GeneratedDocument.Issuer}",
                            "Thành công",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"❌ Lỗi khi tạo văn bản:\n{ex.Message}",
                        "Lỗi",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }
    }

    private void EditTemplate_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string id)
        {
            var template = _documentService.GetTemplateById(id);
            if (template != null)
            {
                var dialog = new TemplateEditDialog(template, _documentService);
                if (dialog.ShowDialog() == true)
                {
                    LoadTemplates();
                }
            }
        }
    }

    private void DeleteTemplate_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string id)
        {
            var template = _documentService.GetTemplateById(id);
            if (template != null)
            {
                var result = MessageBox.Show(
                    $"Bạn có chắc muốn xóa mẫu '{template.Name}'?",
                    "Xác nhận xóa",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    _documentService.DeleteTemplate(id);
                    LoadTemplates();
                    MessageBox.Show("✅ Đã xóa mẫu!", "Thành công",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
    }

    private void Template_DoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (dgTemplates.SelectedItem is DocumentTemplate template)
        {
            var viewer = new TemplateViewDialog(template);
            viewer.ShowDialog();
        }
    }
}
