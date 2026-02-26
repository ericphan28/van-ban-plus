using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace AIVanBan.Desktop.Views;

public partial class HelpPage : Page
{
    private readonly Dictionary<string, FrameworkElement> _sections = new();
    private readonly List<(string SectionName, string Text)> _searchableItems = new();
    private bool _searchIndexBuilt = false;
    private string? _scrollToOnLoad = null;

    public HelpPage()
    {
        InitializeComponent();
        IndexSections();
        Loaded += HelpPage_Loaded;
    }

    /// <summary>Navigate to a specific section when creating the page</summary>
    public HelpPage(string sectionName) : this()
    {
        _scrollToOnLoad = sectionName;
    }

    private void HelpPage_Loaded(object sender, RoutedEventArgs e)
    {
        // Build search index AFTER visual tree is fully loaded
        if (!_searchIndexBuilt)
        {
            BuildSearchIndex(helpContent);
            _searchIndexBuilt = true;
        }

        // Scroll to requested section if specified
        if (!string.IsNullOrEmpty(_scrollToOnLoad))
        {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, () =>
            {
                ScrollToSection(_scrollToOnLoad);
                _scrollToOnLoad = null;
            });
        }
    }

    /// <summary>
    /// Index all named sections for TOC navigation
    /// </summary>
    private void IndexSections()
    {
        var sectionNames = new[]
        {
            "secOverview", "secFirstSetup", "secInterface",
            "secDocManage", "secDocAdd", "secDocFolder", "secDocSearch", "secDocAttach", "secDocExport",
            "secRegister", "secTemplate", "secLegalRef", "secStatistics",
            "secAISetup", "secAICompose", "secAIScan", "secAIReview", "secAIAdvisory", "secAISummary", "secAIReport",
            "secAlbum", "secVietGovCMS", "secMeeting", "secCalendar",
            "secBackup", "secPlans", "secFAQ", "secContact", "secShortcuts",
            "secWhatsNew"
        };

        foreach (var name in sectionNames)
        {
            var element = this.FindName(name) as FrameworkElement;
            if (element != null)
            {
                _sections[name] = element;
            }
        }
    }

    /// <summary>
    /// Build search index by walking the visual tree (must be called after Loaded)
    /// </summary>
    private void BuildSearchIndex(DependencyObject parent)
    {
        if (parent == null) return;

        int childCount;
        try { childCount = VisualTreeHelper.GetChildrenCount(parent); }
        catch { return; }

        for (int i = 0; i < childCount; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);

            if (child is TextBlock tb)
            {
                // Extract full text including from Runs, Bolds, Hyperlinks etc.
                var fullText = GetTextBlockFullText(tb);
                if (!string.IsNullOrWhiteSpace(fullText))
                {
                    var sectionName = FindParentSectionName(tb);
                    if (sectionName != null)
                    {
                        _searchableItems.Add((sectionName, fullText.ToLowerInvariant()));
                    }
                }
            }

            BuildSearchIndex(child);
        }
    }

    /// <summary>
    /// Extract all text from a TextBlock including inline elements (Run, Bold, Italic, Hyperlink)
    /// </summary>
    private static string GetTextBlockFullText(TextBlock tb)
    {
        if (tb.Inlines.Count == 0)
            return tb.Text ?? "";

        var parts = new List<string>();
        foreach (var inline in tb.Inlines)
        {
            parts.Add(GetInlineText(inline));
        }
        return string.Join("", parts);
    }

    private static string GetInlineText(Inline inline)
    {
        if (inline is Run run)
            return run.Text ?? "";
        if (inline is Span span) // Bold, Italic, Underline, Hyperlink are all Span
        {
            var parts = new List<string>();
            foreach (var child in span.Inlines)
                parts.Add(GetInlineText(child));
            return string.Join("", parts);
        }
        return "";
    }

    /// <summary>Find parent section name by walking up visual + logical tree</summary>
    private string? FindParentSectionName(DependencyObject element)
    {
        var current = element;
        while (current != null)
        {
            if (current is FrameworkElement fe && !string.IsNullOrEmpty(fe.Name) && fe.Name.StartsWith("sec"))
            {
                return fe.Name;
            }
            // Try visual parent first, then logical parent
            var parent = VisualTreeHelper.GetParent(current);
            if (parent == null)
                parent = LogicalTreeHelper.GetParent(current);
            current = parent;
        }
        return null;
    }

    // ═══════════════════════════════════════
    // TOC Navigation
    // ═══════════════════════════════════════

    private void TocItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string sectionName)
        {
            // Clear search when navigating via TOC
            if (!string.IsNullOrEmpty(txtSearch.Text))
            {
                txtSearch.Text = "";
            }
            ScrollToSection(sectionName);
        }
    }

    private void ScrollToSection(string sectionName)
    {
        if (_sections.TryGetValue(sectionName, out var element))
        {
            // Ensure section is visible (might be hidden by search)
            element.Visibility = Visibility.Visible;
            element.BringIntoView();
            HighlightSection(element);
        }
    }

    private void HighlightSection(FrameworkElement element)
    {
        if (element is StackPanel panel)
        {
            var originalBg = panel.Background;
            var highlightColor = new SolidColorBrush(Color.FromArgb(40, 33, 150, 243));
            panel.Background = highlightColor;

            // Animate fade out
            var animation = new ColorAnimation
            {
                From = Color.FromArgb(40, 33, 150, 243),
                To = Colors.Transparent,
                Duration = TimeSpan.FromMilliseconds(1200),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            animation.Completed += (s, ev) => panel.Background = originalBg;
            highlightColor.BeginAnimation(SolidColorBrush.ColorProperty, animation);
        }
    }

    // ═══════════════════════════════════════
    // Search
    // ═══════════════════════════════════════

    private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
    {
        var query = txtSearch.Text?.Trim().ToLowerInvariant() ?? "";

        btnClearSearch.Visibility = string.IsNullOrEmpty(query)
            ? Visibility.Collapsed
            : Visibility.Visible;

        if (string.IsNullOrEmpty(query) || query.Length < 2)
        {
            // Show all sections & update result count
            foreach (var section in _sections.Values)
                section.Visibility = Visibility.Visible;
            txtSearchResult.Visibility = Visibility.Collapsed;
            return;
        }

        // Find matching sections
        var matchedSections = new HashSet<string>();
        foreach (var item in _searchableItems)
        {
            if (item.Text.Contains(query))
                matchedSections.Add(item.SectionName);
        }

        // Also match section names with TOC button text
        foreach (var child in tocPanel.Children)
        {
            if (child is Button btn && btn.Tag is string tag)
            {
                var btnText = GetAllTextFromElement(btn).ToLowerInvariant();
                if (btnText.Contains(query) && _sections.ContainsKey(tag))
                    matchedSections.Add(tag);
            }
        }

        // Show/hide sections
        foreach (var kvp in _sections)
        {
            kvp.Value.Visibility = matchedSections.Contains(kvp.Key)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        // Show result count
        txtSearchResult.Text = matchedSections.Count > 0
            ? $"Tìm thấy {matchedSections.Count} mục"
            : "Không tìm thấy kết quả";
        txtSearchResult.Foreground = matchedSections.Count > 0
            ? new SolidColorBrush(Color.FromRgb(76, 175, 80))
            : new SolidColorBrush(Color.FromRgb(244, 67, 54));
        txtSearchResult.Visibility = Visibility.Visible;

        // Scroll to first match
        if (matchedSections.Count > 0)
        {
            var firstMatch = _sections.FirstOrDefault(s => matchedSections.Contains(s.Key));
            firstMatch.Value?.BringIntoView();
        }
    }

    private static string GetAllTextFromElement(DependencyObject element)
    {
        var texts = new List<string>();
        CollectText(element, texts);
        return string.Join(" ", texts);
    }

    private static void CollectText(DependencyObject element, List<string> texts)
    {
        if (element is TextBlock tb)
        {
            var text = GetTextBlockFullText(tb);
            if (!string.IsNullOrWhiteSpace(text))
                texts.Add(text);
        }

        int count;
        try { count = VisualTreeHelper.GetChildrenCount(element); }
        catch { return; }

        for (int i = 0; i < count; i++)
        {
            CollectText(VisualTreeHelper.GetChild(element, i), texts);
        }
    }

    private void ClearSearch_Click(object sender, RoutedEventArgs e)
    {
        txtSearch.Text = "";
        txtSearch.Focus();
    }

    // ═══════════════════════════════════════
    // Scroll to top
    // ═══════════════════════════════════════

    private void ScrollToTop_Click(object sender, RoutedEventArgs e)
    {
        helpScrollViewer.ScrollToTop();
    }

    // ═══════════════════════════════════════
    // Links
    // ═══════════════════════════════════════

    private void OpenWebsite_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("https://giakiemso.com")
            {
                UseShellExecute = true
            });
        }
        catch { }
    }

    private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
    {
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri)
            {
                UseShellExecute = true
            });
            e.Handled = true;
        }
        catch { }
    }

}
