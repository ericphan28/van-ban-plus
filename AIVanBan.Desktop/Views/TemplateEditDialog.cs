using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AIVanBan.Core.Models;
using AIVanBan.Core.Services;

namespace AIVanBan.Desktop.Views;

public partial class TemplateEditDialog : Window
{
    private readonly DocumentService _documentService;
    public new DocumentTemplate? Template { get; private set; }

    private ComboBox cboType = null!;
    private TextBox txtName = null!;
    private TextBox txtCategory = null!;
    private TextBox txtDescription = null!;
    private TextBox txtTags = null!;
    private TextBox txtContent = null!;

    // Placeholder text mẫu cho trường nội dung
    private const string ContentPlaceholder =
        "Nhập nội dung mẫu văn bản tại đây.\n\n" +
        "💡 GỢI Ý: Dùng {tên_biến} cho phần cần thay đổi.\n\n" +
        "VÍ DỤ mẫu Công văn:\n" +
        "──────────────────────\n" +
        "Kính gửi: {nơi_nhận}\n\n" +
        "Căn cứ {căn_cứ_pháp_lý};\n" +
        "Xét đề nghị của {đơn_vị_đề_nghị},\n\n" +
        "{nội_dung_chính}\n\n" +
        "Đề nghị {nơi_nhận} quan tâm, phối hợp thực hiện./.";

    public TemplateEditDialog(DocumentTemplate? template, DocumentService documentService)
    {
        _documentService = documentService;
        Template = template;

        Title = template == null ? "➕ Thêm mẫu mới" : "✏️ Sửa mẫu văn bản";
        Width = 780;
        Height = 720;
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
        var mainGrid = new Grid { Margin = new Thickness(20) };
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });  // 0: Hướng dẫn
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });  // 1: Type + Category (2 cột)
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });  // 2: Name
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });  // 3: Description
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });  // 4: Tags
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // 5: Content
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });  // 6: Buttons

        // ═══════════════════════════════════════════════════════════
        // Row 0: Hướng dẫn nhanh (banner)
        // ═══════════════════════════════════════════════════════════
        var guideBorder = new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(232, 245, 233)), // Xanh nhạt
            BorderBrush = new SolidColorBrush(Color.FromRgb(129, 199, 132)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(12, 8, 12, 8),
            Margin = new Thickness(0, 0, 0, 15)
        };
        var guideText = new TextBlock
        {
            Text = "💡 Hướng dẫn: Chỉ cần nhập Tên mẫu và Nội dung mẫu (có dấu *). " +
                   "Dùng {tên_biến} trong nội dung cho phần cần thay đổi khi sử dụng. " +
                   "Các trường còn lại là tùy chọn giúp phân loại và tìm kiếm nhanh hơn.",
            TextWrapping = TextWrapping.Wrap,
            FontSize = 12.5,
            Foreground = new SolidColorBrush(Color.FromRgb(46, 125, 50))
        };
        guideBorder.Child = guideText;
        Grid.SetRow(guideBorder, 0);

        // ═══════════════════════════════════════════════════════════
        // Row 1: Loại văn bản + Phân loại (2 cột)
        // ═══════════════════════════════════════════════════════════
        var row1Grid = new Grid { Margin = new Thickness(0, 0, 0, 12) };
        row1Grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        row1Grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12, GridUnitType.Pixel) }); // gap
        row1Grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        // Left: Loại văn bản
        var typePanel = new StackPanel();
        typePanel.Children.Add(CreateLabel("Loại văn bản"));
        cboType = new ComboBox { Height = 32 };
        cboType.DisplayMemberPath = "Value";
        cboType.SelectedValuePath = "Key";
        foreach (var item in EnumDisplayHelper.GetDocumentTypeItems())
        {
            cboType.Items.Add(item);
        }
        cboType.SelectedIndex = 0;
        typePanel.Children.Add(cboType);
        Grid.SetColumn(typePanel, 0);

        // Right: Phân loại (Category)
        var catPanel = new StackPanel();
        catPanel.Children.Add(CreateLabel("Phân loại", "(tùy chọn, VD: Nội vụ, Tài chính...)"));
        txtCategory = new TextBox { Height = 32, VerticalContentAlignment = VerticalAlignment.Center };
        catPanel.Children.Add(txtCategory);
        Grid.SetColumn(catPanel, 2);

        row1Grid.Children.Add(typePanel);
        row1Grid.Children.Add(catPanel);
        Grid.SetRow(row1Grid, 1);

        // ═══════════════════════════════════════════════════════════
        // Row 2: Tên mẫu (bắt buộc)
        // ═══════════════════════════════════════════════════════════
        var namePanel = new StackPanel { Margin = new Thickness(0, 0, 0, 12) };
        namePanel.Children.Add(CreateLabel("Tên mẫu *", null, true));
        txtName = new TextBox { Height = 32, VerticalContentAlignment = VerticalAlignment.Center };
        namePanel.Children.Add(txtName);
        Grid.SetRow(namePanel, 2);

        // ═══════════════════════════════════════════════════════════
        // Row 3: Mô tả (tùy chọn)
        // ═══════════════════════════════════════════════════════════
        var descPanel = new StackPanel { Margin = new Thickness(0, 0, 0, 12) };
        descPanel.Children.Add(CreateLabel("Mô tả", "(tùy chọn — giúp tìm kiếm và phân biệt mẫu)"));
        txtDescription = new TextBox
        {
            Height = 50,
            TextWrapping = TextWrapping.Wrap,
            AcceptsReturn = true,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };
        descPanel.Children.Add(txtDescription);
        Grid.SetRow(descPanel, 3);

        // ═══════════════════════════════════════════════════════════
        // Row 4: Tags (tùy chọn)
        // ═══════════════════════════════════════════════════════════
        var tagsPanel = new StackPanel { Margin = new Thickness(0, 0, 0, 12) };
        tagsPanel.Children.Add(CreateLabel("Từ khóa", "(tùy chọn — phân cách bằng dấu phẩy, VD: điều động, cán bộ, nhân sự)"));
        txtTags = new TextBox { Height = 32, VerticalContentAlignment = VerticalAlignment.Center };
        tagsPanel.Children.Add(txtTags);
        Grid.SetRow(tagsPanel, 4);

        // ═══════════════════════════════════════════════════════════
        // Row 5: Nội dung mẫu (bắt buộc, chiếm phần còn lại)
        // ═══════════════════════════════════════════════════════════
        var contentPanel = new DockPanel { Margin = new Thickness(0, 0, 0, 10) };

        var contentHeader = new DockPanel { Margin = new Thickness(0, 0, 0, 5) };
        contentHeader.Children.Add(CreateLabel("Nội dung mẫu *", null, true));

        // Nút chèn {biến} nhanh
        var btnInsertVar = new Button
        {
            Content = "📎 Chèn {biến}",
            FontSize = 11,
            Height = 24,
            Padding = new Thickness(8, 2, 8, 2),
            HorizontalAlignment = HorizontalAlignment.Right,
            ToolTip = "Chèn dấu {tên_biến} vào vị trí con trỏ"
        };
        btnInsertVar.Click += InsertVariable_Click;
        DockPanel.SetDock(btnInsertVar, Dock.Right);
        contentHeader.Children.Add(btnInsertVar);
        DockPanel.SetDock(contentHeader, Dock.Top);
        contentPanel.Children.Add(contentHeader);

        txtContent = new TextBox
        {
            TextWrapping = TextWrapping.Wrap,
            AcceptsReturn = true,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            FontFamily = new FontFamily("Consolas"),
            FontSize = 12.5,
            Padding = new Thickness(8)
        };
        // Hiển thị placeholder khi trống
        txtContent.GotFocus += ContentBox_GotFocus;
        txtContent.LostFocus += ContentBox_LostFocus;
        contentPanel.Children.Add(txtContent);
        Grid.SetRow(contentPanel, 5);

        // ═══════════════════════════════════════════════════════════
        // Row 6: Buttons
        // ═══════════════════════════════════════════════════════════
        var btnPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 10, 0, 0)
        };

        var btnSave = new Button
        {
            Content = "💾 Lưu mẫu",
            Width = 120,
            Height = 36,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 10, 0)
        };
        btnSave.Click += Save_Click;

        var btnCancel = new Button
        {
            Content = "Hủy",
            Width = 90,
            Height = 36
        };
        btnCancel.Click += (s, e) => Close();

        btnPanel.Children.Add(btnSave);
        btnPanel.Children.Add(btnCancel);
        Grid.SetRow(btnPanel, 6);

        // Add all rows to grid
        mainGrid.Children.Add(guideBorder);
        mainGrid.Children.Add(row1Grid);
        mainGrid.Children.Add(namePanel);
        mainGrid.Children.Add(descPanel);
        mainGrid.Children.Add(tagsPanel);
        mainGrid.Children.Add(contentPanel);
        mainGrid.Children.Add(btnPanel);

        Content = mainGrid;

        // Hiển thị placeholder cho nội dung khi tạo mới
        if (Template == null)
        {
            ShowContentPlaceholder();
        }
    }

    /// <summary>
    /// Tạo label có hint phụ (tùy chọn) — giúp phân biệt bắt buộc vs tùy chọn
    /// </summary>
    private UIElement CreateLabel(string text, string? hint = null, bool isRequired = false)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 4) };

        var lbl = new TextBlock
        {
            Text = text,
            FontWeight = isRequired ? FontWeights.SemiBold : FontWeights.Normal,
            FontSize = 13,
            Foreground = isRequired
                ? new SolidColorBrush(Color.FromRgb(33, 33, 33))
                : new SolidColorBrush(Color.FromRgb(97, 97, 97))
        };
        panel.Children.Add(lbl);

        if (!string.IsNullOrEmpty(hint))
        {
            var hintBlock = new TextBlock
            {
                Text = "  " + hint,
                FontSize = 11.5,
                Foreground = new SolidColorBrush(Color.FromRgb(158, 158, 158)),
                VerticalAlignment = VerticalAlignment.Bottom
            };
            panel.Children.Add(hintBlock);
        }

        return panel;
    }

    /// <summary>
    /// Hiển thị placeholder cho trường nội dung
    /// </summary>
    private void ShowContentPlaceholder()
    {
        txtContent.Text = ContentPlaceholder;
        txtContent.Foreground = new SolidColorBrush(Color.FromRgb(180, 180, 180));
    }

    private void ContentBox_GotFocus(object sender, RoutedEventArgs e)
    {
        if (txtContent.Text == ContentPlaceholder)
        {
            txtContent.Text = "";
            txtContent.Foreground = new SolidColorBrush(Colors.Black);
        }
    }

    private void ContentBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtContent.Text))
        {
            ShowContentPlaceholder();
        }
    }

    /// <summary>
    /// Chèn {tên_biến} vào vị trí con trỏ trong trường nội dung
    /// </summary>
    private void InsertVariable_Click(object sender, RoutedEventArgs e)
    {
        // Xóa placeholder nếu đang hiển thị
        if (txtContent.Text == ContentPlaceholder)
        {
            txtContent.Text = "";
            txtContent.Foreground = new SolidColorBrush(Colors.Black);
        }

        var caretIndex = txtContent.CaretIndex;
        var varName = "{tên_biến}";
        txtContent.Text = txtContent.Text.Insert(caretIndex, varName);
        txtContent.CaretIndex = caretIndex + varName.Length;
        txtContent.Focus();
    }

    private void LoadTemplate(DocumentTemplate template)
    {
        cboType.SelectedValue = template.Type;
        txtName.Text = template.Name;
        txtCategory.Text = template.Category ?? "";
        txtDescription.Text = template.Description;
        txtTags.Text = template.Tags != null ? string.Join(", ", template.Tags) : "";
        txtContent.Text = template.TemplateContent;
        txtContent.Foreground = new SolidColorBrush(Colors.Black);
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(txtName.Text))
        {
            MessageBox.Show("Vui lòng nhập tên mẫu!", "Thiếu thông tin",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            txtName.Focus();
            return;
        }

        var contentText = txtContent.Text;
        if (string.IsNullOrWhiteSpace(contentText) || contentText == ContentPlaceholder)
        {
            MessageBox.Show("Vui lòng nhập nội dung mẫu!", "Thiếu thông tin",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            txtContent.Focus();
            return;
        }

        // Parse tags từ chuỗi phân cách bằng dấu phẩy
        string[]? tags = null;
        if (!string.IsNullOrWhiteSpace(txtTags.Text))
        {
            tags = txtTags.Text
                .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim())
                .Where(t => !string.IsNullOrEmpty(t))
                .ToArray();
        }

        if (Template == null)
        {
            // Create new
            Template = new DocumentTemplate
            {
                Type = (DocumentType)cboType.SelectedValue,
                Name = txtName.Text.Trim(),
                Category = string.IsNullOrWhiteSpace(txtCategory.Text) ? null : txtCategory.Text.Trim(),
                Description = txtDescription.Text.Trim(),
                Tags = tags,
                TemplateContent = contentText.Trim(),
                CreatedBy = Environment.UserName,
                CreatedDate = DateTime.Now
            };
            _documentService.AddTemplate(Template);
        }
        else
        {
            // Update existing
            Template.Type = (DocumentType)cboType.SelectedValue;
            Template.Name = txtName.Text.Trim();
            Template.Category = string.IsNullOrWhiteSpace(txtCategory.Text) ? null : txtCategory.Text.Trim();
            Template.Description = txtDescription.Text.Trim();
            Template.Tags = tags;
            Template.TemplateContent = contentText.Trim();
            _documentService.UpdateTemplate(Template);
        }

        DialogResult = true;
        Close();
    }
}
