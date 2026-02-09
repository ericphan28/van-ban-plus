using System;
using System.Windows;
using System.Windows.Controls;
using AIVanBan.Core.Models;
using AIVanBan.Core.Services;

namespace AIVanBan.Desktop.Views;

public partial class TemplateEditDialog : Window
{
    private readonly DocumentService _documentService;
    public new DocumentTemplate? Template { get; private set; }

    private ComboBox cboType = null!;
    private TextBox txtName = null!;
    private TextBox txtDescription = null!;
    private TextBox txtContent = null!;

    public TemplateEditDialog(DocumentTemplate? template, DocumentService documentService)
    {
        _documentService = documentService;
        Template = template;
        
        Title = template == null ? "Thêm mẫu mới" : "Sửa mẫu văn bản";
        Width = 700;
        Height = 650;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        ResizeMode = ResizeMode.CanResize;
        
        BuildUI();
        
        if (template != null)
        {
            LoadTemplate(template);
        }
    }

    private void BuildUI()
    {
        var grid = new Grid { Margin = new Thickness(20) };
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        // Type
        var lblType = new TextBlock { Text = "Loại văn bản:", Margin = new Thickness(0, 0, 0, 5) };
        cboType = new ComboBox { Margin = new Thickness(0, 0, 0, 15) };
        foreach (DocumentType type in Enum.GetValues(typeof(DocumentType)))
        {
            cboType.Items.Add(type);
        }
        cboType.SelectedIndex = 0;

        var typePanel = new StackPanel();
        typePanel.Children.Add(lblType);
        typePanel.Children.Add(cboType);
        Grid.SetRow(typePanel, 0);

        // Name
        var lblName = new TextBlock { Text = "Tên mẫu: *", Margin = new Thickness(0, 0, 0, 5) };
        txtName = new TextBox { Margin = new Thickness(0, 0, 0, 15) };
        
        var namePanel = new StackPanel();
        namePanel.Children.Add(lblName);
        namePanel.Children.Add(txtName);
        Grid.SetRow(namePanel, 1);

        // Description
        var lblDesc = new TextBlock { Text = "Mô tả:", Margin = new Thickness(0, 0, 0, 5) };
        txtDescription = new TextBox 
        { 
            Margin = new Thickness(0, 0, 0, 15),
            Height = 60,
            TextWrapping = TextWrapping.Wrap,
            AcceptsReturn = true,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };
        
        var descPanel = new StackPanel();
        descPanel.Children.Add(lblDesc);
        descPanel.Children.Add(txtDescription);
        Grid.SetRow(descPanel, 2);

        // Content
        var lblContent = new TextBlock { Text = "Nội dung mẫu: *", Margin = new Thickness(0, 0, 0, 5) };
        txtContent = new TextBox 
        { 
            Margin = new Thickness(0, 0, 0, 15),
            TextWrapping = TextWrapping.Wrap,
            AcceptsReturn = true,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            FontFamily = new System.Windows.Media.FontFamily("Consolas"),
            FontSize = 12
        };
        
        var contentPanel = new StackPanel();
        contentPanel.Children.Add(lblContent);
        contentPanel.Children.Add(txtContent);
        Grid.SetRow(contentPanel, 3);

        // Buttons
        var btnPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 15, 0, 0)
        };

        var btnSave = new Button 
        { 
            Content = "💾 Lưu", 
            Width = 100, 
            Height = 35,
            Margin = new Thickness(0, 0, 10, 0) 
        };
        btnSave.Click += Save_Click;

        var btnCancel = new Button 
        { 
            Content = "Hủy", 
            Width = 100,
            Height = 35
        };
        btnCancel.Click += (s, e) => Close();

        btnPanel.Children.Add(btnSave);
        btnPanel.Children.Add(btnCancel);
        Grid.SetRow(btnPanel, 4);

        grid.Children.Add(typePanel);
        grid.Children.Add(namePanel);
        grid.Children.Add(descPanel);
        grid.Children.Add(contentPanel);
        grid.Children.Add(btnPanel);

        Content = grid;
    }

    private void LoadTemplate(DocumentTemplate template)
    {
        cboType.SelectedItem = template.Type;
        txtName.Text = template.Name;
        txtDescription.Text = template.Description;
        txtContent.Text = template.TemplateContent;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(txtName.Text))
        {
            MessageBox.Show("Vui lòng nhập tên mẫu!", "Lỗi", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(txtContent.Text))
        {
            MessageBox.Show("Vui lòng nhập nội dung mẫu!", "Lỗi", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (Template == null)
        {
            // Create new
            Template = new DocumentTemplate
            {
                Type = (DocumentType)cboType.SelectedItem,
                Name = txtName.Text.Trim(),
                Description = txtDescription.Text.Trim(),
                TemplateContent = txtContent.Text.Trim(),
                CreatedBy = Environment.UserName,
                CreatedDate = DateTime.Now
            };
            _documentService.AddTemplate(Template);
        }
        else
        {
            // Update existing
            Template.Type = (DocumentType)cboType.SelectedItem;
            Template.Name = txtName.Text.Trim();
            Template.Description = txtDescription.Text.Trim();
            Template.TemplateContent = txtContent.Text.Trim();
            _documentService.UpdateTemplate(Template);
        }

        DialogResult = true;
        Close();
    }
}
