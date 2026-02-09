using System;
using System.Linq;
using System.Windows;
using AIVanBan.Core.Models;
using AIVanBan.Core.Services;

namespace AIVanBan.Desktop.Views;

public partial class CreateAlbumFromTemplateDialog : Window
{
    private readonly AlbumStructureTemplate _template;
    private readonly AlbumStructureService _albumService;
    private AlbumCategory? _selectedCategory;
    private AlbumSubCategory? _selectedSubCategory;

    public AlbumInstance? CreatedAlbum { get; private set; }

    public CreateAlbumFromTemplateDialog(AlbumStructureTemplate template, AlbumStructureService albumService)
    {
        InitializeComponent();
        _template = template;
        _albumService = albumService;
        
        LoadCategories();
    }

    private void LoadCategories()
    {
        cboCategory.ItemsSource = _template.Categories.OrderBy(c => c.SortOrder);
    }

    private void Category_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (cboCategory.SelectedItem is AlbumCategory category)
        {
            _selectedCategory = category;
            cboSubCategory.ItemsSource = category.SubCategories.OrderBy(s => s.SortOrder);
            cboSubCategory.IsEnabled = true;
            cboSubCategory.SelectedIndex = -1;
            _selectedSubCategory = null;
            UpdatePreview();
        }
    }

    private void SubCategory_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (cboSubCategory.SelectedItem is AlbumSubCategory subCategory)
        {
            _selectedSubCategory = subCategory;
            
            // Show description
            if (!string.IsNullOrEmpty(subCategory.Description))
            {
                txtCategoryInfo.Text = $"ℹ️ {subCategory.Description}";
                txtCategoryInfo.Visibility = Visibility.Visible;
            }
            else
            {
                txtCategoryInfo.Visibility = Visibility.Collapsed;
            }
            
            // Show suggested tags
            if (subCategory.SuggestedTags.Length > 0)
            {
                icTags.ItemsSource = subCategory.SuggestedTags;
                pnlSuggestedTags.Visibility = Visibility.Visible;
            }
            else
            {
                pnlSuggestedTags.Visibility = Visibility.Collapsed;
            }
            
            // Show auto-year option if applicable
            chkAutoCreateYear.Visibility = subCategory.AutoCreateYearFolder 
                ? Visibility.Visible 
                : Visibility.Collapsed;
            
            if (subCategory.AutoCreateYearFolder)
            {
                chkAutoCreateYear.IsChecked = true;
            }
            
            UpdatePreview();
        }
    }

    private void UpdatePreview()
    {
        if (_selectedCategory == null || _selectedSubCategory == null || string.IsNullOrWhiteSpace(txtAlbumName.Text))
        {
            txtPreviewPath.Text = "Chọn danh mục và nhập tên album...";
            btnCreate.IsEnabled = false;
            return;
        }

        var albumName = txtAlbumName.Text.Trim();
        var yearPart = chkAutoCreateYear.IsChecked == true ? $"\\{DateTime.Now.Year}" : "";
        var path = $"Photos\\{_selectedCategory.Name}\\{_selectedSubCategory.Name}{yearPart}\\{albumName}";
        
        txtPreviewPath.Text = path;
        btnCreate.IsEnabled = !string.IsNullOrWhiteSpace(albumName);
    }

    private void Create_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedCategory == null || _selectedSubCategory == null)
        {
            MessageBox.Show("Vui lòng chọn danh mục và phân loại!", "Thông báo", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var albumName = txtAlbumName.Text.Trim();
        if (string.IsNullOrWhiteSpace(albumName))
        {
            MessageBox.Show("Vui lòng nhập tên album!", "Thông báo", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            btnCreate.IsEnabled = false;
            btnCreate.Content = "⏳ Đang tạo...";

            // Build full album name with year if needed
            var fullAlbumName = albumName;
            if (chkAutoCreateYear.IsChecked == true && _selectedSubCategory.AutoCreateYearFolder)
            {
                fullAlbumName = $"{DateTime.Now.Year}/{albumName}";
            }

            // Create album instance
            CreatedAlbum = _albumService.CreateAlbum(
                _selectedCategory.Id,
                _selectedSubCategory.Id,
                fullAlbumName,
                txtDescription.Text.Trim()
            );

            // Update additional properties
            if (dpEventDate.SelectedDate.HasValue)
            {
                CreatedAlbum.EventDate = dpEventDate.SelectedDate.Value;
            }

            if (!string.IsNullOrWhiteSpace(txtLocation.Text))
            {
                CreatedAlbum.Location = txtLocation.Text.Trim();
            }

            // Save to database (update)
            var albums = _albumService.GetAllAlbums();
            // Update would happen here if we had an UpdateAlbum method

            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"❌ Lỗi tạo album:\n{ex.Message}",
                "Lỗi",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            
            btnCreate.IsEnabled = true;
            btnCreate.Content = "TẠO ALBUM";
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void txtAlbumName_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        UpdatePreview();
    }
}
