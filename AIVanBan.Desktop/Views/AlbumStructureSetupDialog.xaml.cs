using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AIVanBan.Core.Models;
using AIVanBan.Core.Services;
using MaterialDesignThemes.Wpf;

namespace AIVanBan.Desktop.Views;

public partial class AlbumStructureSetupDialog : Window
{
    private readonly AlbumStructureService _albumService;
    private AlbumStructureTemplate? _selectedTemplate;

    public AlbumStructureSetupDialog(AlbumStructureService albumService)
    {
        InitializeComponent();
        _albumService = albumService;
        LoadTemplates();
    }

    private void LoadTemplates()
    {
        var templates = _albumService.GetAllTemplates();
        lvTemplates.ItemsSource = templates;

        // Auto-select active template
        var activeTemplate = templates.FirstOrDefault(t => t.IsActive);
        if (activeTemplate != null)
        {
            lvTemplates.SelectedItem = activeTemplate;
        }
    }

    private void Templates_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (lvTemplates.SelectedItem is AlbumStructureTemplate template)
        {
            _selectedTemplate = template;
            ShowTemplatePreview(template);
            btnCreateStructure.IsEnabled = true;
        }
    }

    private void ShowTemplatePreview(AlbumStructureTemplate template)
    {
        // Update info
        txtTemplateName.Text = template.Name;
        txtTemplateDescription.Text = template.Description;

        // Stats
        var totalSubCategories = template.Categories.Sum(c => c.SubCategories.Count);
        chipCategories.Content = $"{template.Categories.Count} danh mục chính";
        chipSubCategories.Content = $"{totalSubCategories} phân loại";
        pnlTemplateStats.Visibility = Visibility.Visible;

        // Build tree
        tvStructurePreview.Items.Clear();

        var rootNode = new TreeViewItem
        {
            Header = CreateTreeHeader("🖼️", "ALBUM ẢNH", $"{template.Categories.Count} danh mục"),
            IsExpanded = true,
            FontWeight = FontWeights.Bold
        };

        foreach (var category in template.Categories.OrderBy(c => c.SortOrder))
        {
            var categoryNode = new TreeViewItem
            {
                Header = CreateTreeHeader(
                    category.Icon, 
                    category.Name, 
                    $"{category.SubCategories.Count} phân loại"),
                IsExpanded = true,
                FontWeight = FontWeights.SemiBold
            };

            foreach (var subCategory in category.SubCategories.OrderBy(s => s.SortOrder))
            {
                var subNode = new TreeViewItem
                {
                    Header = CreateTreeHeader(
                        subCategory.Icon,
                        subCategory.Name,
                        subCategory.AutoCreateYearFolder ? "Tự động tạo folder năm" : ""),
                    ToolTip = CreateSubCategoryTooltip(subCategory)
                };

                categoryNode.Items.Add(subNode);
            }

            rootNode.Items.Add(categoryNode);
        }

        tvStructurePreview.Items.Add(rootNode);
    }

    private UIElement CreateTreeHeader(string icon, string name, string info)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal };

        // Icon
        panel.Children.Add(new TextBlock
        {
            Text = icon,
            FontSize = 16,
            Margin = new Thickness(0, 0, 8, 0),
            VerticalAlignment = VerticalAlignment.Center
        });

        // Name
        panel.Children.Add(new TextBlock
        {
            Text = name,
            FontSize = 13,
            VerticalAlignment = VerticalAlignment.Center
        });

        // Info
        if (!string.IsNullOrEmpty(info))
        {
            panel.Children.Add(new TextBlock
            {
                Text = $" ({info})",
                FontSize = 11,
                Foreground = System.Windows.Media.Brushes.Gray,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5, 0, 0, 0)
            });
        }

        return panel;
    }

    private string CreateSubCategoryTooltip(AlbumSubCategory subCategory)
    {
        var tooltip = subCategory.Name;
        
        if (!string.IsNullOrEmpty(subCategory.Description))
            tooltip += $"\n{subCategory.Description}";

        if (subCategory.SuggestedTags.Length > 0)
            tooltip += $"\n\nTags gợi ý: {string.Join(", ", subCategory.SuggestedTags)}";

        if (subCategory.AutoCreateYearFolder)
            tooltip += "\n\n✅ Tự động tạo folder theo năm";

        return tooltip;
    }

    private async void CreateStructure_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedTemplate == null) return;

        // Ask for organization name
        var orgNameDialog = new OrganizationNameInputDialog();
        orgNameDialog.Owner = this;  // Set owner để dialog hiển thị giữa parent window
        if (orgNameDialog.ShowDialog() != true) return;
        
        var organizationName = orgNameDialog.OrganizationName;

        var result = MessageBox.Show(
            $"Bạn có chắc muốn áp dụng cấu trúc:\n\n" +
            $"🏢 Tổ chức: {organizationName}\n" +
            $"📋 Template: {_selectedTemplate.Name}\n" +
            $"📂 {_selectedTemplate.Categories.Count} danh mục chính\n" +
            $"📁 {_selectedTemplate.Categories.Sum(c => c.SubCategories.Count)} phân loại\n\n" +
            $"Hệ thống sẽ tạo cấu trúc thư mục theo mô hình:\n" +
            $"📁 {organizationName}\n" +
            $"  📁 {_selectedTemplate.Categories.FirstOrDefault()?.Name ?? "Category"}\n" +
            $"    📁 {_selectedTemplate.Categories.FirstOrDefault()?.SubCategories.FirstOrDefault()?.Name ?? "SubCategory"}\n\n" +
            $"Tiếp tục?",
            "Xác nhận",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        try
        {
            btnCreateStructure.IsEnabled = false;
            btnCreateStructure.Content = "⏳ Đang tạo cấu trúc...";

            // Set as active template
            _albumService.SetActiveTemplate(_selectedTemplate.Id);

            // Create folder structure using AlbumFolderService
            await System.Threading.Tasks.Task.Run(() =>
            {
                using (var folderService = new AlbumFolderService())
                {
                    // Apply template to create folder tree
                    folderService.ApplyTemplate(_selectedTemplate, organizationName);
                }
                
                // Also create physical structure for backward compatibility
                _albumService.CreatePhysicalStructure(_selectedTemplate);
            });

            MessageBox.Show(
                $"✅ Tạo cấu trúc album thành công!\n\n" +
                $"📁 Tổ chức: {organizationName}\n" +
                $"📂 {_selectedTemplate.Categories.Count} danh mục chính\n" +
                $"📁 {_selectedTemplate.Categories.Sum(c => c.SubCategories.Count)} phân loại\n\n" +
                $"Bạn có thể bắt đầu tạo album trong các thư mục.",
                "Thành công",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"❌ Lỗi khi tạo cấu trúc:\n{ex.Message}",
                "Lỗi",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            btnCreateStructure.IsEnabled = true;
            btnCreateStructure.Content = "✅ Áp dụng cấu trúc này";
        }
    }

    private async void SyncFromWeb_Click(object sender, RoutedEventArgs e)
    {
        // Dialog to enter sync URL
        var dialog = new SyncUrlInputDialog();
        if (dialog.ShowDialog() != true) return;

        var syncUrl = dialog.SyncUrl;
        var organizationType = dialog.OrganizationType;

        try
        {
            btnSyncFromWeb.IsEnabled = false;
            btnSyncFromWeb.Content = "⏳ Đang đồng bộ...";

            var template = await _albumService.SyncTemplateFromWeb(syncUrl, organizationType);

            if (template != null)
            {
                MessageBox.Show(
                    $"✅ Đồng bộ thành công!\n\n" +
                    $"📋 {template.Name}\n" +
                    $"🔢 Version: {template.Version}\n" +
                    $"📅 {template.LastSyncDate:dd/MM/yyyy HH:mm}",
                    "Thành công",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                LoadTemplates();
            }
            else
            {
                MessageBox.Show(
                    "❌ Không thể tải template từ server.\nVui lòng kiểm tra URL và kết nối mạng.",
                    "Lỗi",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"❌ Lỗi đồng bộ:\n{ex.Message}",
                "Lỗi",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            btnSyncFromWeb.IsEnabled = true;
            btnSyncFromWeb.Content = "🌐 Đồng bộ từ Web";
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}

/// <summary>
/// Dialog nhập URL đồng bộ
/// </summary>
public class SyncUrlInputDialog : Window
{
    private TextBox txtUrl;
    private ComboBox cboOrganizationType;

    public string SyncUrl => txtUrl.Text.Trim();
    public string OrganizationType => (cboOrganizationType.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "XaPhuong";

    public SyncUrlInputDialog()
    {
        Title = "Đồng bộ từ Web";
        Width = 500;
        Height = 250;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        ResizeMode = ResizeMode.NoResize;

        var grid = new Grid { Margin = new Thickness(20) };
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        // Title
        var title = new TextBlock
        {
            Text = "🌐 Đồng bộ cấu trúc Album từ Web",
            FontSize = 16,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 0, 0, 20)
        };
        Grid.SetRow(title, 0);
        grid.Children.Add(title);

        // Organization Type
        var lblOrg = new TextBlock { Text = "Loại cơ quan:", Margin = new Thickness(0, 0, 0, 5) };
        Grid.SetRow(lblOrg, 1);
        grid.Children.Add(lblOrg);

        cboOrganizationType = new ComboBox { Margin = new Thickness(0, 0, 0, 15) };
        cboOrganizationType.Items.Add(new ComboBoxItem { Content = "UBND Xã/Phường", Tag = "XaPhuong" });
        cboOrganizationType.Items.Add(new ComboBoxItem { Content = "UBND Huyện", Tag = "Huyen" });
        cboOrganizationType.Items.Add(new ComboBoxItem { Content = "UBND Tỉnh", Tag = "Tinh" });
        cboOrganizationType.Items.Add(new ComboBoxItem { Content = "Hội Nông dân", Tag = "HoiNongDan" });
        cboOrganizationType.SelectedIndex = 0;
        Grid.SetRow(cboOrganizationType, 2);
        grid.Children.Add(cboOrganizationType);

        // URL
        var lblUrl = new TextBlock { Text = "URL API:", Margin = new Thickness(0, 0, 0, 5) };
        Grid.SetRow(lblUrl, 3);
        grid.Children.Add(lblUrl);

        txtUrl = new TextBox
        {
            Text = "https://api.example.com/album-templates",
            Margin = new Thickness(0, 0, 0, 20)
        };
        Grid.SetRow(txtUrl, 4);
        grid.Children.Add(txtUrl);

        // Buttons
        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right
        };

        var btnOk = new Button
        {
            Content = "ĐỒNG BỘ",
            Width = 100,
            Margin = new Thickness(0, 0, 10, 0)
        };
        btnOk.Click += (s, e) =>
        {
            if (string.IsNullOrWhiteSpace(txtUrl.Text))
            {
                MessageBox.Show("Vui lòng nhập URL!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            DialogResult = true;
            Close();
        };
        buttonPanel.Children.Add(btnOk);

        var btnCancel = new Button { Content = "HỦY", Width = 100 };
        btnCancel.Click += (s, e) => { DialogResult = false; Close(); };
        buttonPanel.Children.Add(btnCancel);

        Grid.SetRow(buttonPanel, 5);
        grid.Children.Add(buttonPanel);

        Content = grid;
    }
}

/// <summary>
/// Dialog nhập tên tổ chức khi apply template - THIẾT KẾ ĐƠN GIẢN
/// </summary>
public class OrganizationNameInputDialog : Window
{
    private TextBox txtOrgName;

    public string OrganizationName => txtOrgName.Text.Trim();

    public OrganizationNameInputDialog()
    {
        Title = "Nhập tên tổ chức";
        Width = 550;
        Height = 400;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        ResizeMode = ResizeMode.NoResize;
        Background = System.Windows.Media.Brushes.White;

        // Main Grid với 3 rows: Header | Content | Buttons
        var mainGrid = new Grid();
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Header
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Content
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Buttons

        // ============ HEADER ============
        var headerPanel = new StackPanel
        {
            Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(33, 150, 243)),
            Margin = new Thickness(0, 0, 0, 0)
        };

        var headerInner = new StackPanel
        {
            Margin = new Thickness(30, 25, 30, 25)
        };

        var iconText = new TextBlock
        {
            Text = "🏢",
            FontSize = 48,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 12)
        };
        headerInner.Children.Add(iconText);

        var titleText = new TextBlock
        {
            Text = "TÊN TỔ CHỨC/ĐƠN VỊ",
            FontSize = 22,
            FontWeight = FontWeights.Bold,
            Foreground = System.Windows.Media.Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 8)
        };
        headerInner.Children.Add(titleText);

        var descText = new TextBlock
        {
            Text = "Tên này sẽ làm thư mục gốc cho cấu trúc album",
            FontSize = 13,
            Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(220, 235, 255)),
            HorizontalAlignment = HorizontalAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
            TextAlignment = TextAlignment.Center
        };
        headerInner.Children.Add(descText);

        headerPanel.Children.Add(headerInner);
        Grid.SetRow(headerPanel, 0);
        mainGrid.Children.Add(headerPanel);

        // ============ CONTENT ============
        var contentPanel = new StackPanel
        {
            Margin = new Thickness(35, 25, 35, 25),
            VerticalAlignment = VerticalAlignment.Center
        };

        var exampleText = new TextBlock
        {
            Text = "💡 Ví dụ: Trường Tiểu học Lê Quý Đôn, UBND Xã Hòa Bình...",
            FontSize = 12,
            Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(120, 120, 120)),
            Margin = new Thickness(0, 0, 0, 10)
        };
        contentPanel.Children.Add(exampleText);

        txtOrgName = new TextBox
        {
            Text = "Trường Tiểu học",
            FontSize = 16,
            Padding = new Thickness(15, 12, 15, 12),
            BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(33, 150, 243)),
            BorderThickness = new Thickness(2)
        };
        contentPanel.Children.Add(txtOrgName);
        
        Grid.SetRow(contentPanel, 1);
        mainGrid.Children.Add(contentPanel);

        // ============ BUTTONS - TRONG GRID RIÊNG ============
        var buttonsContainer = new Border
        {
            Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(250, 250, 250)),
            BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(230, 230, 230)),
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(20)
        };
        
        var buttonsPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        // Button XÁC NHẬN - XANH LÁ TO
        var btnConfirm = new Button
        {
            Width = 170,
            Height = 50,
            Margin = new Thickness(0, 0, 15, 0),
            Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(76, 175, 80)),
            Foreground = System.Windows.Media.Brushes.White,
            BorderThickness = new Thickness(0),
            FontSize = 15,
            FontWeight = FontWeights.Bold,
            Cursor = System.Windows.Input.Cursors.Hand,
            IsDefault = true
        };

        var btnConfirmContent = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        btnConfirmContent.Children.Add(new TextBlock { Text = "✓", FontSize = 20, Margin = new Thickness(0, 0, 8, 0) });
        btnConfirmContent.Children.Add(new TextBlock { Text = "XÁC NHẬN", FontSize = 15, FontWeight = FontWeights.Bold });
        btnConfirm.Content = btnConfirmContent;

        btnConfirm.MouseEnter += (s, e) => btnConfirm.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(56, 142, 60));
        btnConfirm.MouseLeave += (s, e) => btnConfirm.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(76, 175, 80));

        btnConfirm.Click += (s, e) =>
        {
            if (string.IsNullOrWhiteSpace(txtOrgName.Text))
            {
                MessageBox.Show("⚠️ Vui lòng nhập tên tổ chức!", "Thiếu thông tin",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtOrgName.Focus();
                return;
            }
            DialogResult = true;
            Close();
        };
        buttonsPanel.Children.Add(btnConfirm);

        // Button HỦY - TRẮNG VIỀN XÁM
        var btnCancel = new Button
        {
            Width = 170,
            Height = 50,
            Background = System.Windows.Media.Brushes.White,
            Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(80, 80, 80)),
            BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(180, 180, 180)),
            BorderThickness = new Thickness(2),
            FontSize = 15,
            FontWeight = FontWeights.Bold,
            Cursor = System.Windows.Input.Cursors.Hand,
            IsCancel = true
        };

        var btnCancelContent = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        btnCancelContent.Children.Add(new TextBlock { Text = "✕", FontSize = 20, Margin = new Thickness(0, 0, 8, 0) });
        btnCancelContent.Children.Add(new TextBlock { Text = "HỦY BỎ", FontSize = 15, FontWeight = FontWeights.Bold });
        btnCancel.Content = btnCancelContent;

        btnCancel.MouseEnter += (s, e) => btnCancel.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(245, 245, 245));
        btnCancel.MouseLeave += (s, e) => btnCancel.Background = System.Windows.Media.Brushes.White;

        btnCancel.Click += (s, e) =>
        {
            DialogResult = false;
            Close();
        };
        buttonsPanel.Children.Add(btnCancel);

        buttonsContainer.Child = buttonsPanel;
        Grid.SetRow(buttonsContainer, 2);
        mainGrid.Children.Add(buttonsContainer);

        Content = mainGrid;

        Loaded += (s, e) =>
        {
            txtOrgName.Focus();
            txtOrgName.SelectAll();
        };
    }
}
