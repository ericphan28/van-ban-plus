using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AIVanBan.Core.Models;
using AIVanBan.Core.Services;

namespace AIVanBan.Desktop.Views;

public class AlbumSelectorDialog : Window
{
    private readonly DocumentService _documentService;
    private TreeView albumTree = null!;
    public string? SelectedAlbumId { get; private set; }

    public AlbumSelectorDialog(DocumentService documentService)
    {
        _documentService = documentService;
        
        Title = "Chọn album đích";
        Width = 400;
        Height = 500;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        ResizeMode = ResizeMode.CanResize;

        BuildUI();
        LoadAlbums();
    }

    public class AlbumNode
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Icon { get; set; } = "📁";
        public ObservableCollection<AlbumNode> Children { get; set; } = new();
    }

    private void BuildUI()
    {
        var grid = new Grid { Margin = new Thickness(15) };
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        // Header
        var header = new TextBlock
        {
            Text = "Chọn album để di chuyển ảnh vào:",
            FontSize = 14,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 10)
        };
        Grid.SetRow(header, 0);

        // TreeView
        var scroll = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        albumTree = new TreeView { BorderThickness = new Thickness(1) };
        
        var template = new HierarchicalDataTemplate { ItemsSource = new System.Windows.Data.Binding("Children") };
        var stackPanel = new FrameworkElementFactory(typeof(StackPanel));
        stackPanel.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
        
        var iconText = new FrameworkElementFactory(typeof(TextBlock));
        iconText.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding("Icon"));
        iconText.SetValue(TextBlock.FontSizeProperty, 16.0);
        iconText.SetValue(TextBlock.MarginProperty, new Thickness(0, 0, 8, 0));
        
        var nameText = new FrameworkElementFactory(typeof(TextBlock));
        nameText.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding("Name"));
        nameText.SetValue(TextBlock.FontSizeProperty, 13.0);
        
        stackPanel.AppendChild(iconText);
        stackPanel.AppendChild(nameText);
        template.VisualTree = stackPanel;
        
        albumTree.ItemTemplate = template;
        scroll.Content = albumTree;
        Grid.SetRow(scroll, 1);

        // Buttons
        var btnPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 15, 0, 0)
        };
        Grid.SetRow(btnPanel, 2);

        var btnOk = new Button
        {
            Content = "✓ Chọn",
            Width = 120,
            Height = 35,
            Margin = new Thickness(0, 0, 10, 0),
            FontSize = 14,
            Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(76, 175, 80))
        };
        btnOk.Click += Ok_Click;

        var btnCancel = new Button
        {
            Content = "Hủy",
            Width = 120,
            Height = 35,
            FontSize = 14
        };
        btnCancel.Click += (s, e) => Close();

        btnPanel.Children.Add(btnOk);
        btnPanel.Children.Add(btnCancel);

        grid.Children.Add(header);
        grid.Children.Add(scroll);
        grid.Children.Add(btnPanel);

        Content = grid;
    }

    private void LoadAlbums()
    {
        albumTree.Items.Clear();

        // Root
        var rootNode = new AlbumNode
        {
            Id = "",
            Name = "Gốc (không album)",
            Icon = "🏠"
        };

        var albums = _documentService.GetAllAlbums();
        BuildTree(rootNode, albums, string.Empty);

        albumTree.Items.Add(rootNode);
    }

    private void BuildTree(AlbumNode parentNode, System.Collections.Generic.List<Album> albums, string parentId)
    {
        var children = albums.Where(a => a.ParentId == parentId).ToList();
        foreach (var album in children)
        {
            var node = new AlbumNode
            {
                Id = album.Id,
                Name = album.Name,
                Icon = album.Icon
            };

            BuildTree(node, albums, album.Id);
            parentNode.Children.Add(node);
        }
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        var selectedNode = albumTree.SelectedItem as AlbumNode;
        if (selectedNode == null)
        {
            MessageBox.Show("Vui lòng chọn album!", "Thông báo",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        SelectedAlbumId = selectedNode.Id;
        DialogResult = true;
        Close();
    }
}
