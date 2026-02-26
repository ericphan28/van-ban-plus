using System;
using System.Windows;
using System.Windows.Controls;
using AIVanBan.Core.Models;
using AIVanBan.Core.Services;

namespace AIVanBan.Desktop.Views;

public partial class PhotoEditDialog : Window
{
    private readonly DocumentService _documentService;
    private readonly Photo _photo;

    private TextBox txtTitle = null!;
    private TextBox txtDescription = null!;
    private TextBox txtLocation = null!;
    private TextBox txtTags = null!;
    private DatePicker dpTaken = null!;

    public PhotoEditDialog(Photo photo, DocumentService documentService)
    {
        _photo = photo;
        _documentService = documentService;

        Title = "Sửa thông tin ảnh";
        Width = 500;
        Height = 500;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        ResizeMode = ResizeMode.CanResizeWithGrip;

        BuildUI();
        LoadPhoto();
    }

    private void BuildUI()
    {
        var grid = new Grid { Margin = new Thickness(20) };
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var stack = new StackPanel();

        // Event/Title
        stack.Children.Add(new TextBlock { Text = "Tiêu đề/Sự kiện:", Margin = new Thickness(0, 0, 0, 5) });
        txtTitle = new TextBox { Margin = new Thickness(0, 0, 0, 15) };
        stack.Children.Add(txtTitle);

        // Description
        stack.Children.Add(new TextBlock { Text = "Mô tả:", Margin = new Thickness(0, 0, 0, 5) });
        txtDescription = new TextBox 
        { 
            Margin = new Thickness(0, 0, 0, 15),
            Height = 60,
            TextWrapping = TextWrapping.Wrap,
            AcceptsReturn = true
        };
        stack.Children.Add(txtDescription);

        // Location
        stack.Children.Add(new TextBlock { Text = "Địa điểm:", Margin = new Thickness(0, 0, 0, 5) });
        txtLocation = new TextBox { Margin = new Thickness(0, 0, 0, 15) };
        stack.Children.Add(txtLocation);

        // Tags
        stack.Children.Add(new TextBlock { Text = "Tags (cách nhau bằng dấu phẩy):", Margin = new Thickness(0, 0, 0, 5) });
        txtTags = new TextBox { Margin = new Thickness(0, 0, 0, 15) };
        stack.Children.Add(txtTags);

        // Date
        stack.Children.Add(new TextBlock { Text = "Ngày chụp:", Margin = new Thickness(0, 0, 0, 5) });
        dpTaken = new DatePicker { Margin = new Thickness(0, 0, 0, 15) };
        stack.Children.Add(dpTaken);

        // Buttons
        var btnPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 15, 0, 0)
        };

        var btnSave = new Button { Content = "💾 Lưu", MinWidth = 100, Height = 36, Padding = new Thickness(16, 0, 16, 0), VerticalContentAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 10, 0) };
        btnSave.Click += Save_Click;

        var btnCancel = new Button { Content = "Hủy", MinWidth = 100, Height = 36, Padding = new Thickness(16, 0, 16, 0), VerticalContentAlignment = VerticalAlignment.Center };
        btnCancel.Click += (s, e) => Close();

        btnPanel.Children.Add(btnSave);
        btnPanel.Children.Add(btnCancel);
        stack.Children.Add(btnPanel);

        grid.Children.Add(stack);
        Content = grid;
    }

    private void LoadPhoto()
    {
        txtTitle.Text = _photo.Event;
        txtDescription.Text = _photo.Description;
        txtLocation.Text = _photo.Location;
        txtTags.Text = string.Join(", ", _photo.Tags);
        dpTaken.SelectedDate = _photo.DateTaken;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        _photo.Event = txtTitle.Text.Trim();
        _photo.Description = txtDescription.Text.Trim();
        _photo.Location = txtLocation.Text.Trim();
        _photo.Tags = txtTags.Text.Split(',', StringSplitOptions.RemoveEmptyEntries)
                              .Select(t => t.Trim())
                              .ToArray();
        _photo.DateTaken = dpTaken.SelectedDate ?? DateTime.Now;

        _documentService.UpdatePhoto(_photo);
        DialogResult = true;
        Close();
    }
}
