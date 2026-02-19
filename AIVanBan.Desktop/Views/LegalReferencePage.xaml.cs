using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using AIVanBan.Core.Models;

namespace AIVanBan.Desktop.Views
{
    /// <summary>
    /// Trang tra cứu pháp quy — NĐ 30/2020/NĐ-CP
    /// Theo Điều 1, NĐ 30/2020/NĐ-CP
    /// </summary>
    public partial class LegalReferencePage : Page
    {
        private readonly DispatcherTimer _searchDebounce;
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
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu pháp quy:\n{ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

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

            // Nội dung
            txtArticleContent.Text = node.Content;

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
