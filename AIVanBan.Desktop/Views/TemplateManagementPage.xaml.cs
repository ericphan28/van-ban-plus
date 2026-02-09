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
            cboFilterType.Items.Add(type);
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
        if (cboFilterType.SelectedIndex > 0)
        {
            var selectedType = (DocumentType)cboFilterType.SelectedItem;
            filtered = filtered.Where(t => t.Type == selectedType);
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
                // Increment usage count
                template.UsageCount++;
                _documentService.UpdateTemplate(template);
                
                // Navigate to AI Generator with this template pre-loaded
                MessageBox.Show($"Sử dụng mẫu: {template.Name}\n\nChức năng này sẽ chuyển đến trang AI Generator và tự động load mẫu.", 
                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
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
