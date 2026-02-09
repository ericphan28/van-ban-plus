using System;
using System.Windows;
using System.Windows.Controls;
using AIVanBan.Core.Models;
using AIVanBan.Core.Services;

namespace AIVanBan.Desktop.Views;

public partial class AlbumEditDialog : Window
{
    private readonly DocumentService _documentService;
    private Album? _album;

    private TextBox txtName = null!;
    private TextBox txtDescription = null!;

    public AlbumEditDialog(Album? album, DocumentService documentService)
    {
        _album = album;
        _documentService = documentService;

        Title = album == null ? "Tạo album mới" : "Sửa album";
        Width = 450;
        Height = 300;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        ResizeMode = ResizeMode.NoResize;

        BuildUI();
        
        if (album != null)
        {
            LoadAlbum();
        }
    }

    private void BuildUI()
    {
        var grid = new Grid { Margin = new Thickness(20) };
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        
        // Name
        var lblName = new TextBlock 
        { 
            Text = "Tên album:", 
            Margin = new Thickness(0, 0, 0, 5),
            FontWeight = FontWeights.SemiBold
        };
        Grid.SetRow(lblName, 0);
        
        txtName = new TextBox 
        { 
            Margin = new Thickness(0, 0, 0, 20),
            FontSize = 14,
            Padding = new Thickness(8)
        };
        Grid.SetRow(txtName, 1);

        // Description
        var descPanel = new StackPanel();
        Grid.SetRow(descPanel, 2);
        
        descPanel.Children.Add(new TextBlock 
        { 
            Text = "Mô tả:", 
            Margin = new Thickness(0, 0, 0, 5),
            FontWeight = FontWeights.SemiBold
        });
        
        txtDescription = new TextBox 
        { 
            Height = 80,
            TextWrapping = TextWrapping.Wrap,
            AcceptsReturn = true,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            FontSize = 14,
            Padding = new Thickness(8)
        };
        descPanel.Children.Add(txtDescription);

        // Buttons
        var btnPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 20, 0, 0)
        };
        Grid.SetRow(btnPanel, 3);

        var btnSave = new Button 
        { 
            Content = "💾 Lưu", 
            Width = 120, 
            Height = 35,
            Margin = new Thickness(0, 0, 10, 0),
            FontSize = 14,
            FontWeight = FontWeights.SemiBold,
            Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(33, 150, 243))
        };
        btnSave.Click += Save_Click;

        var btnCancel = new Button 
        { 
            Content = "❌ Hủy", 
            Width = 120,
            Height = 35,
            FontSize = 14
        };
        btnCancel.Click += (s, e) => Close();

        btnPanel.Children.Add(btnSave);
        btnPanel.Children.Add(btnCancel);

        grid.Children.Add(lblName);
        grid.Children.Add(txtName);
        grid.Children.Add(descPanel);
        grid.Children.Add(btnPanel);
        
        Content = grid;
    }

    private void LoadAlbum()
    {
        if (_album != null)
        {
            txtName.Text = _album.Name;
            txtDescription.Text = _album.Description;
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtName.Text))
        {
            MessageBox.Show("Vui lòng nhập tên album!", "Lỗi",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (_album == null)
        {
            _album = new Album
            {
                Name = txtName.Text.Trim(),
                Description = txtDescription.Text.Trim(),
                CreatedDate = DateTime.Now
            };
            _documentService.AddAlbum(_album);
        }
        else
        {
            _album.Name = txtName.Text.Trim();
            _album.Description = txtDescription.Text.Trim();
            _documentService.UpdateAlbum(_album);
        }

        DialogResult = true;
        Close();
    }
}
