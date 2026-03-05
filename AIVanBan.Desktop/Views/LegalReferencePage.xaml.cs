using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using AIVanBan.Core.Models;
using AIVanBan.Core.Services;

namespace AIVanBan.Desktop.Views
{
    /// <summary>
    /// Trang tra cứu pháp quy — NĐ 30/2020/NĐ-CP
    /// Hỗ trợ kiểm tra cập nhật pháp quy từ VanBanPlus API (tương tự Template Store)
    /// Theo Điều 1, NĐ 30/2020/NĐ-CP
    /// </summary>
    public partial class LegalReferencePage : Page
    {
        private readonly DispatcherTimer _searchDebounce;
        private readonly LegalUpdateService _legalUpdateService = new();
        private List<LegalNode> _legalTree = new();

        // Ánh xạ tag → tên tính năng tiếng Việt
        private static readonly Dictionary<string, string> FeatureTagNames = new()
        {
            { "DocumentType", "Loại văn bản" },
            { "DocumentEdit", "Soạn thảo VB" },
            { "DocumentList", "Danh sách VB" },
            { "AICompose", "AI Soạn thảo" },
            { "AIReview", "Kiểm tra VB" },
            { "Template", "Mẫu VB" },
            { "Signing", "Ký ban hành" },
            { "Register", "Sổ đăng ký" },
            { "CopyDocument", "Sao VB" },
            { "Backup", "Sao lưu" },
            { "AutoIncrement", "Cấp số tự động" },
            { "Glossary", "Thuật ngữ" },
        };

        public LegalReferencePage()
        {
            InitializeComponent();

            // Debounce timer cho search (300ms delay)
            _searchDebounce = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(300)
            };
            _searchDebounce.Tick += SearchDebounce_Tick;

            LoadLegalTree();
        }

        /// <summary>
        /// Constructor cho phép nhảy thẳng đến Điều cụ thể
        /// </summary>
        public LegalReferencePage(int articleNumber) : this()
        {
            // Delay navigation để tree load xong
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
            {
                NavigateToArticle(articleNumber);
            }));
        }

        /// <summary>
        /// Constructor cho phép nhảy thẳng đến Phụ lục cụ thể
        /// </summary>
        public LegalReferencePage(string appendixRoman) : this()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
            {
                NavigateToAppendix(appendixRoman);
            }));
        }

        #region Load Data

        private void LoadLegalTree()
        {
            try
            {
                _legalTree = LegalReferenceData.GetLegalTree();
                treeViewLegal.ItemsSource = _legalTree;

                // Mở rộng node gốc
                if (treeViewLegal.Items.Count > 0)
                {
                    Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
                    {
                        var container = treeViewLegal.ItemContainerGenerator.ContainerFromIndex(0) as TreeViewItem;
                        if (container != null)
                        {
                            container.IsExpanded = true;
                        }
                    }));
                }
                
                // Hiển thị thời gian kiểm tra cập nhật lần cuối
                txtLastChecked.Text = LegalUpdateService.GetLastCheckedText();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu pháp quy:\n{ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Legal Update — Kiểm tra cập nhật pháp quy từ server

        /// <summary>
        /// Kiểm tra cập nhật pháp quy từ VanBanPlus API (tương tự tải Template Store)
        /// </summary>
        private async void BtnCheckUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // UI: Đang kiểm tra
                btnCheckUpdate.IsEnabled = false;
                txtUpdateStatus.Text = "Đang kiểm tra...";
                iconUpdate.Kind = MaterialDesignThemes.Wpf.PackIconKind.CloudSync;
                
                var status = await _legalUpdateService.CheckForUpdatesAsync();
                
                txtLastChecked.Text = $"Kiểm tra: {DateTime.Now:dd/MM/yyyy HH:mm}";
                
                if (status.HasUpdate)
                {
                    iconUpdate.Kind = MaterialDesignThemes.Wpf.PackIconKind.CloudDownload;
                    txtUpdateStatus.Text = "Có bản mới!";
                    
                    var result = MessageBox.Show(
                        $"🆕 Có cập nhật pháp quy mới!\n\n" +
                        $"Phiên bản server: v{status.ServerManifestVersion}\n" +
                        $"Phiên bản local: v{status.LocalManifestVersion}\n" +
                        $"Cập nhật ngày: {status.ServerUpdatedAt}\n\n" +
                        $"Văn bản có sẵn:\n" +
                        string.Join("\n", status.AvailableDocuments.Select(d => 
                            $"  • {d.Title} ({d.Articles} Điều, {d.Appendices} Phụ lục)")) +
                        $"\n\n{status.Notice}" +
                        $"\n\nBạn có muốn tải về không?",
                        "Cập nhật Pháp quy — VanBanPlus",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Information);
                    
                    if (result == MessageBoxResult.Yes)
                    {
                        txtUpdateStatus.Text = "Đang tải...";
                        var success = await _legalUpdateService.DownloadLatestManifestAsync();
                        
                        if (success)
                        {
                            iconUpdate.Kind = MaterialDesignThemes.Wpf.PackIconKind.CloudCheckOutline;
                            txtUpdateStatus.Text = "✅ Đã cập nhật";
                            txtLastChecked.Text = LegalUpdateService.GetLastCheckedText();
                            
                            MessageBox.Show(
                                "✅ Đã cập nhật dữ liệu pháp quy thành công!\n\n" +
                                "Dữ liệu hiện tại đã là phiên bản mới nhất.",
                                "Cập nhật thành công",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                        }
                        else
                        {
                            txtUpdateStatus.Text = "Lỗi tải về";
                            MessageBox.Show("Không thể tải dữ liệu. Vui lòng thử lại sau.",
                                "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                    else
                    {
                        txtUpdateStatus.Text = "Có bản mới";
                    }
                }
                else
                {
                    iconUpdate.Kind = MaterialDesignThemes.Wpf.PackIconKind.CloudCheckOutline;
                    txtUpdateStatus.Text = "✅ Đã mới nhất";
                    
                    // Lưu manifest để cập nhật last-checked
                    await _legalUpdateService.DownloadLatestManifestAsync();
                    txtLastChecked.Text = LegalUpdateService.GetLastCheckedText();
                    
                    MessageBox.Show(
                        $"✅ Dữ liệu pháp quy đã là phiên bản mới nhất (v{status.ServerManifestVersion}).\n\n" +
                        $"Cập nhật ngày: {status.ServerUpdatedAt}\n" +
                        $"{status.Notice}",
                        "Pháp quy đã cập nhật",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                iconUpdate.Kind = MaterialDesignThemes.Wpf.PackIconKind.CloudOffOutline;
                txtUpdateStatus.Text = "Lỗi kết nối";
                
                MessageBox.Show(
                    $"⚠️ Không thể kiểm tra cập nhật:\n{ex.Message}\n\n" +
                    "Vui lòng kiểm tra kết nối internet và thử lại.",
                    "Lỗi kết nối",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            finally
            {
                btnCheckUpdate.IsEnabled = true;
            }
        }

        #endregion

        #endregion

        #region TreeView Events

        private void TreeViewLegal_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is LegalNode node)
            {
                DisplayNode(node);
            }
        }

        private void DisplayNode(LegalNode node)
        {
            // Hiện panel nội dung, ẩn welcome
            pnlWelcome.Visibility = Visibility.Collapsed;
            pnlArticleContent.Visibility = Visibility.Visible;

            // Badge loại
            txtNodeType.Text = GetNodeTypeName(node.NodeType);

            // Tiêu đề
            txtArticleTitle.Text = node.Title;

            // Nội dung (rich rendering: tables, bullets, headers)
            RenderRichContent(pnlRichContent, node.Content);

            // Breadcrumb
            txtBreadcrumb.Text = BuildBreadcrumb(node);

            // Feature tags
            if (node.FeatureTags.Count > 0)
            {
                pnlFeatureTags.Visibility = Visibility.Visible;
                icFeatureTags.ItemsSource = node.FeatureTags
                    .Select(t => FeatureTagNames.TryGetValue(t, out var name) ? name : t)
                    .ToList();
            }
            else
            {
                pnlFeatureTags.Visibility = Visibility.Collapsed;
            }

            // Nội dung con
            if (node.Children.Count > 0)
            {
                pnlChildrenSummary.Visibility = Visibility.Visible;
                icChildren.ItemsSource = node.Children;
            }
            else
            {
                pnlChildrenSummary.Visibility = Visibility.Collapsed;
            }
        }

        // Cached brushes cho rich content (resolve 1 lần, dùng lại)
        private Brush? _primaryBrush, _primaryLightBrush, _dividerBrush, _bodyBrush;

        private Brush GetBrush(string key, Brush fallback)
        {
            return TryFindResource(key) as Brush
                ?? Application.Current.TryFindResource(key) as Brush
                ?? fallback;
        }

        private void EnsureBrushes()
        {
            _primaryBrush ??= GetBrush("PrimaryHueMidBrush", new SolidColorBrush(Color.FromRgb(25, 118, 210)));
            _primaryLightBrush ??= GetBrush("PrimaryHueLightBrush", new SolidColorBrush(Color.FromRgb(227, 242, 253)));
            _dividerBrush ??= GetBrush("MaterialDesignDivider", new SolidColorBrush(Color.FromRgb(224, 224, 224)));
            _bodyBrush ??= GetBrush("MaterialDesignBody", new SolidColorBrush(Color.FromRgb(33, 33, 33)));
        }

        /// <summary>
        /// Render nội dung rich: bảng |...|, bullet •, header IN HOA, numbered items
        /// </summary>
        private void RenderRichContent(StackPanel panel, string? content)
        {
            panel.Children.Clear();
            EnsureBrushes();
            if (string.IsNullOrWhiteSpace(content)) return;

            var lines = content.Split('\n');
            var tableRows = new List<string[]>();

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var trimmed = line.Trim();

                // Empty line → spacer (flush table first)
                if (string.IsNullOrWhiteSpace(trimmed))
                {
                    if (tableRows.Count > 0) { FlushTable(panel, tableRows); tableRows.Clear(); }
                    panel.Children.Add(new Border { Height = 6 });
                    continue;
                }

                // Table row: |col|col|
                if (trimmed.StartsWith('|') && trimmed.EndsWith('|'))
                {
                    // Skip separator rows like |---|---|
                    if (trimmed.Contains("---")) continue;

                    var cells = trimmed.Split('|')
                        .Skip(1).SkipLast(1)
                        .Select(c => c.Trim())
                        .ToArray();
                    tableRows.Add(cells);
                    continue;
                }

                // Flush pending table before other content
                if (tableRows.Count > 0) { FlushTable(panel, tableRows); tableRows.Clear(); }

                // Header detection: ALL CAPS text (>3 chars, not bullet)
                bool isAllCaps = trimmed.Length > 3 && !trimmed.StartsWith("•") &&
                    trimmed == trimmed.ToUpperInvariant() &&
                    trimmed.Any(char.IsLetter);

                // Section header: e.g. "I. NGUYÊN TẮC..." or "BỐ CỤC:" or "PHẦN I —"
                bool isSectionHeader = !trimmed.StartsWith("•") && !trimmed.StartsWith("(") &&
                    (isAllCaps ||
                     (trimmed.EndsWith(":") && trimmed.Length < 80) ||
                     (trimmed.StartsWith("Phần ") && trimmed.Contains(".")));

                if (isSectionHeader)
                {
                    panel.Children.Add(new TextBlock
                    {
                        Text = trimmed,
                        FontSize = 14,
                        FontWeight = FontWeights.Bold,
                        Margin = new Thickness(0, 8, 0, 4),
                        TextWrapping = TextWrapping.Wrap,
                        Foreground = _primaryBrush
                    });
                }
                // Bullet: • or - at start
                else if (trimmed.StartsWith("•") || (trimmed.StartsWith("- ") && !trimmed.StartsWith("--")))
                {
                    var bulletText = trimmed.TrimStart('•', '-', ' ');
                    var sp = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(12, 2, 0, 2) };
                    sp.Children.Add(new TextBlock
                    {
                        Text = "•",
                        FontSize = 14,
                        Margin = new Thickness(0, 0, 8, 0),
                        VerticalAlignment = VerticalAlignment.Top,
                        Foreground = _primaryBrush
                    });
                    sp.Children.Add(new TextBlock
                    {
                        Text = bulletText,
                        FontSize = 13.5,
                        TextWrapping = TextWrapping.Wrap,
                        LineHeight = 22,
                        Foreground = _bodyBrush
                    });
                    panel.Children.Add(sp);
                }
                // Numbered item: starts with digit + . or digit + )
                else if (trimmed.Length > 2 && char.IsDigit(trimmed[0]) && (trimmed[1] == '.' || trimmed[1] == ')' ||
                         (char.IsDigit(trimmed[1]) && trimmed.Length > 3 && (trimmed[2] == '.' || trimmed[2] == ')'))))
                {
                    // Find where number ends
                    int dotPos = trimmed.IndexOfAny(new[] { '.', ')' });
                    var numPart = trimmed[..(dotPos + 1)];
                    var textPart = trimmed[(dotPos + 1)..].TrimStart();

                    var sp = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(4, 3, 0, 3) };
                    sp.Children.Add(new TextBlock
                    {
                        Text = numPart,
                        FontSize = 13.5,
                        FontWeight = FontWeights.SemiBold,
                        MinWidth = 28,
                        VerticalAlignment = VerticalAlignment.Top,
                        Foreground = _primaryBrush
                    });
                    sp.Children.Add(new TextBlock
                    {
                        Text = textPart,
                        FontSize = 13.5,
                        TextWrapping = TextWrapping.Wrap,
                        LineHeight = 22,
                        Foreground = _bodyBrush
                    });
                    panel.Children.Add(sp);
                }
                // Lettered item: a) b) c) etc.
                else if (trimmed.Length > 2 && char.IsLetter(trimmed[0]) && trimmed[1] == ')')
                {
                    var sp = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(20, 2, 0, 2) };
                    sp.Children.Add(new TextBlock
                    {
                        Text = trimmed[..2],
                        FontSize = 13.5,
                        FontWeight = FontWeights.SemiBold,
                        MinWidth = 24,
                        VerticalAlignment = VerticalAlignment.Top,
                        Foreground = _primaryBrush
                    });
                    sp.Children.Add(new TextBlock
                    {
                        Text = trimmed[2..].TrimStart(),
                        FontSize = 13.5,
                        TextWrapping = TextWrapping.Wrap,
                        LineHeight = 22,
                        Foreground = _bodyBrush
                    });
                    panel.Children.Add(sp);
                }
                // Regular paragraph
                else
                {
                    panel.Children.Add(new TextBlock
                    {
                        Text = trimmed,
                        FontSize = 13.5,
                        TextWrapping = TextWrapping.Wrap,
                        LineHeight = 24,
                        Margin = new Thickness(0, 2, 0, 2),
                        Foreground = _bodyBrush
                    });
                }
            }

            // Flush remaining table
            if (tableRows.Count > 0) FlushTable(panel, tableRows);
        }

        /// <summary>
        /// Render bảng WPF từ danh sách dòng (row 0 = header)
        /// </summary>
        private void FlushTable(StackPanel panel, List<string[]> rows)
        {
            if (rows.Count == 0) return;

            int maxCols = rows.Max(r => r.Length);
            var grid = new Grid { Margin = new Thickness(0, 8, 0, 8) };

            // Column definitions
            for (int c = 0; c < maxCols; c++)
            {
                // Cột cuối stretch, cột khác auto
                grid.ColumnDefinitions.Add(new ColumnDefinition
                {
                    Width = c < maxCols - 1 ? GridLength.Auto : new GridLength(1, GridUnitType.Star)
                });
            }

            // Row definitions
            for (int r = 0; r < rows.Count; r++)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }

            EnsureBrushes();
            var headerBg = _primaryLightBrush!;
            var borderBrush = _dividerBrush!;
            var bodyFg = _bodyBrush!;

            for (int r = 0; r < rows.Count; r++)
            {
                for (int c = 0; c < maxCols; c++)
                {
                    var cellText = c < rows[r].Length ? rows[r][c] : "";
                    bool isHeader = (r == 0);

                    var border = new Border
                    {
                        BorderBrush = borderBrush,
                        BorderThickness = new Thickness(
                            c == 0 ? 1 : 0, // left
                            r == 0 ? 1 : 0, // top
                            1,              // right
                            1               // bottom
                        ),
                        Padding = new Thickness(10, 6, 10, 6),
                        Background = isHeader ? headerBg : Brushes.Transparent
                    };

                    var tb = new TextBlock
                    {
                        Text = cellText,
                        FontSize = 12.5,
                        FontWeight = isHeader ? FontWeights.Bold : FontWeights.Normal,
                        TextWrapping = TextWrapping.Wrap,
                        Foreground = bodyFg
                    };

                    border.Child = tb;
                    Grid.SetRow(border, r);
                    Grid.SetColumn(border, c);
                    grid.Children.Add(border);
                }
            }

            // Wrap trong border bo tròn
            var wrapper = new Border
            {
                CornerRadius = new CornerRadius(6),
                BorderBrush = borderBrush,
                BorderThickness = new Thickness(0),
                ClipToBounds = true,
                Child = grid,
                Margin = new Thickness(0, 4, 0, 4)
            };

            panel.Children.Add(wrapper);
        }

        private string GetNodeTypeName(LegalNodeType type) => type switch
        {
            LegalNodeType.Document => "📜 Văn bản",
            LegalNodeType.Chapter => "📖 Chương",
            LegalNodeType.Section => "📁 Mục",
            LegalNodeType.Article => "📄 Điều",
            LegalNodeType.Appendix => "📎 Phụ lục",
            LegalNodeType.SubSection => "📋 Phần",
            _ => "📄"
        };

        private string BuildBreadcrumb(LegalNode targetNode)
        {
            var path = new List<string>();
            BuildBreadcrumbRecursive(_legalTree, targetNode, path);
            return string.Join(" › ", path);
        }

        private bool BuildBreadcrumbRecursive(List<LegalNode> nodes, LegalNode target, List<string> path)
        {
            foreach (var node in nodes)
            {
                if (node.Id == target.Id)
                {
                    path.Add(node.Title);
                    return true;
                }

                if (node.Children.Count > 0)
                {
                    path.Add(node.Title);
                    if (BuildBreadcrumbRecursive(node.Children, target, path))
                        return true;
                    path.RemoveAt(path.Count - 1);
                }
            }
            return false;
        }

        private void ChildItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is LegalNode node)
            {
                DisplayNode(node);
                // Cũng select trong tree nếu tìm thấy
                SelectNodeInTree(node);
            }
        }

        #endregion

        #region Search

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            _searchDebounce.Stop();
            _searchDebounce.Start();

            btnClearSearch.Visibility = string.IsNullOrWhiteSpace(txtSearch.Text)
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                _searchDebounce.Stop();
                PerformSearch();
            }
            else if (e.Key == Key.Escape)
            {
                ClearSearch();
            }
        }

        private void SearchDebounce_Tick(object? sender, EventArgs e)
        {
            _searchDebounce.Stop();
            PerformSearch();
        }

        private void PerformSearch()
        {
            var keyword = txtSearch.Text?.Trim() ?? "";

            if (string.IsNullOrEmpty(keyword))
            {
                ClearSearch();
                return;
            }

            var results = LegalReferenceData.Search(keyword);

            // Hiện panel kết quả, ẩn tree
            treeViewLegal.Visibility = Visibility.Collapsed;
            pnlSearchResults.Visibility = Visibility.Visible;

            txtSearchCount.Text = $"🔍 {results.Count} kết quả cho \"{keyword}\"";
            lstSearchResults.ItemsSource = results;

            // Nếu chỉ có 1 kết quả, tự động hiển thị
            if (results.Count == 1)
            {
                DisplayNode(results[0].Node);
            }
        }

        private void ClearSearch()
        {
            txtSearch.Text = "";
            btnClearSearch.Visibility = Visibility.Collapsed;
            pnlSearchResults.Visibility = Visibility.Collapsed;
            treeViewLegal.Visibility = Visibility.Visible;
        }

        private void BtnClearSearch_Click(object sender, RoutedEventArgs e)
        {
            ClearSearch();
            txtSearch.Focus();
        }

        private void LstSearchResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstSearchResults.SelectedItem is LegalSearchResult result)
            {
                DisplayNode(result.Node);
            }
        }

        #endregion

        #region Expand/Collapse

        private void BtnExpandAll_Click(object sender, RoutedEventArgs e)
        {
            SetTreeExpansion(treeViewLegal, true);
        }

        private void BtnCollapseAll_Click(object sender, RoutedEventArgs e)
        {
            SetTreeExpansion(treeViewLegal, false);

            // Mở lại node gốc
            Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                var container = treeViewLegal.ItemContainerGenerator.ContainerFromIndex(0) as TreeViewItem;
                if (container != null) container.IsExpanded = true;
            }));
        }

        private void SetTreeExpansion(ItemsControl control, bool isExpanded)
        {
            foreach (var item in control.Items)
            {
                var container = control.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
                if (container != null)
                {
                    container.IsExpanded = isExpanded;
                    SetTreeExpansion(container, isExpanded);
                }
            }
        }

        #endregion

        #region Navigation helpers (called from other pages)

        /// <summary>
        /// Nhảy đến Điều cụ thể
        /// </summary>
        public void NavigateToArticle(int articleNumber)
        {
            var article = LegalReferenceData.FindArticle(articleNumber);
            if (article != null)
            {
                DisplayNode(article);
                SelectNodeInTree(article);
            }
        }

        /// <summary>
        /// Nhảy đến Phụ lục cụ thể
        /// </summary>
        public void NavigateToAppendix(string romanNumber)
        {
            var appendix = LegalReferenceData.FindAppendix(romanNumber);
            if (appendix != null)
            {
                DisplayNode(appendix);
                SelectNodeInTree(appendix);
            }
        }

        private void SelectNodeInTree(LegalNode targetNode)
        {
            // Mở rộng tree đến node cần tìm
            Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                ExpandAndSelect(treeViewLegal, targetNode);
            }));
        }

        private bool ExpandAndSelect(ItemsControl parent, LegalNode target)
        {
            foreach (var item in parent.Items)
            {
                var container = parent.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
                if (container == null) continue;

                if (item is LegalNode node && node.Id == target.Id)
                {
                    container.IsSelected = true;
                    container.BringIntoView();
                    return true;
                }

                container.IsExpanded = true;
                container.UpdateLayout();

                if (ExpandAndSelect(container, target))
                    return true;

                // Thu gọn lại nếu không tìm thấy trong nhánh này
                container.IsExpanded = false;
            }
            return false;
        }

        #endregion
    }
}
