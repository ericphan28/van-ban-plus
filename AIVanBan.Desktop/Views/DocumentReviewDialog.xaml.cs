using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using AIVanBan.Core.Models;
using AIVanBan.Core.Services;

namespace AIVanBan.Desktop.Views;

public partial class DocumentReviewDialog : Window
{
    private string _content;
    private string _documentType;
    private string _title;
    private string _issuer;
    private DocumentReviewResult? _result;
    private DispatcherTimer? _timer;
    private int _elapsedSeconds;

    /// <summary>
    /// Nội dung đã sửa (nếu user chọn "Áp dụng")
    /// </summary>
    public string? AppliedContent { get; private set; }

    /// <summary>
    /// Constructor duy nhất — luôn hiện ô nhập nội dung trước.
    /// Nếu có content (từ văn bản đã lưu) → điền sẵn, user bấm Kiểm tra.
    /// Nếu content rỗng → user tự dán/nhập text.
    /// </summary>
    public DocumentReviewDialog(string content = "", string documentType = "", string title = "", string issuer = "")
    {
        InitializeComponent();
        _content = content;
        _documentType = documentType;
        _title = title;
        _issuer = issuer;

        // Hiện input panel, ẩn loading
        pnlQuickInput.Visibility = Visibility.Visible;
        pnlLoading.Visibility = Visibility.Collapsed;

        // Header info
        if (!string.IsNullOrWhiteSpace(title))
            txtHeaderInfo.Text = $"📄 {title} — Xem lại nội dung rồi nhấn Kiểm tra";
        else
            txtHeaderInfo.Text = "Dán hoặc chỉnh sửa nội dung rồi nhấn Kiểm tra";

        // Pre-fill content nếu có
        if (!string.IsNullOrWhiteSpace(content))
            txtQuickInput.Text = content;

        // Populate document type ComboBox
        var docTypes = EnumDisplayHelper.GetDocumentTypeItems();
        cboQuickDocType.Items.Add(new { Display = "— Không chọn —", Value = "" });
        foreach (var kv in docTypes)
            cboQuickDocType.Items.Add(new { Display = kv.Value, Value = kv.Value });
        cboQuickDocType.DisplayMemberPath = "Display";
        cboQuickDocType.SelectedValuePath = "Value";
        // Select matching doc type if provided
        cboQuickDocType.SelectedIndex = 0;
        if (!string.IsNullOrWhiteSpace(documentType))
        {
            for (int i = 1; i < cboQuickDocType.Items.Count; i++)
            {
                var item = cboQuickDocType.Items[i];
                var displayProp = item.GetType().GetProperty("Display");
                if (displayProp?.GetValue(item)?.ToString() == documentType)
                {
                    cboQuickDocType.SelectedIndex = i;
                    break;
                }
            }
        }

        // Track char count
        txtQuickInput.TextChanged += (s, e) =>
        {
            var len = txtQuickInput.Text.Length;
            txtCharCount.Text = $"{len:N0} ký tự";
            btnQuickCheck.IsEnabled = len > 10;
        };
        // Trigger initial count
        var initLen = txtQuickInput.Text.Length;
        txtCharCount.Text = $"{initLen:N0} ký tự";
        btnQuickCheck.IsEnabled = initLen > 10;

        this.Height = 780;
    }

    private async Task StartReview()
    {
        ShowLoading(true);
        StartTimer();

        try
        {
            var reviewService = new DocumentReviewService();
            _result = await reviewService.ReviewDocumentAsync(_content, _documentType, _title, _issuer);
            DisplayResults(_result);
        }
        catch (Exception ex)
        {
            ShowError($"Lỗi khi phân tích: {ex.Message}");
        }
        finally
        {
            StopTimer();
        }
    }

    private void DisplayResults(DocumentReviewResult result)
    {
        ShowLoading(false);
        pnlResults.Visibility = Visibility.Visible;
        pnlError.Visibility = Visibility.Collapsed;

        // Score badge
        scoreBadge.Visibility = Visibility.Visible;
        txtScore.Text = result.OverallScore.ToString();
        scoreBadge.Background = new SolidColorBrush(
            (Color)ColorConverter.ConvertFromString(result.ScoreColor));

        // Summary
        txtSummary.Text = result.Summary;
        txtScoreText.Text = $"📊 Đánh giá: {result.OverallScore}/10 — {result.ScoreText}";

        // Severity counts
        txtCriticalCount.Text = $"🔴 {result.CriticalCount} Nghiêm trọng";
        txtWarningCount.Text = $"🟡 {result.WarningCount} Cần xem xét";
        txtSuggestionCount.Text = $"🟢 {result.SuggestionCount} Gợi ý";

        // Strengths
        if (result.Strengths.Count > 0)
        {
            cardStrengths.Visibility = Visibility.Visible;
            lstStrengths.ItemsSource = result.Strengths.Select(s => $"✅ {s}").ToList();
        }

        // Issues list — convert to view models for binding
        var issueVMs = result.Issues.Select(i => new ReviewIssueViewModel(i)).ToList();
        lstIssues.ItemsSource = issueVMs;

        // Suggested content
        if (!string.IsNullOrWhiteSpace(result.SuggestedContent))
        {
            txtSuggestedContent.Text = result.SuggestedContent;
            btnApply.Visibility = Visibility.Visible;
        }
        else
        {
            txtSuggestedContent.Text = "(AI không đề xuất sửa nội dung — văn bản đã tốt hoặc chỉ có lỗi nhỏ)";
        }

        // Show copy button
        btnCopyResult.Visibility = Visibility.Visible;
    }

    private void ShowError(string message)
    {
        ShowLoading(false);
        pnlResults.Visibility = Visibility.Collapsed;
        pnlError.Visibility = Visibility.Visible;
        txtError.Text = message;
    }

    private void ShowLoading(bool show)
    {
        pnlLoading.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        if (show)
        {
            pnlResults.Visibility = Visibility.Collapsed;
            pnlError.Visibility = Visibility.Collapsed;
        }
    }

    private void StartTimer()
    {
        _elapsedSeconds = 0;
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += (s, e) =>
        {
            _elapsedSeconds++;
            txtLoadingTimer.Text = $"⏱️ {_elapsedSeconds} giây...";

            // Update status messages based on elapsed time
            txtLoadingStatus.Text = _elapsedSeconds switch
            {
                <= 5 => "⏳ Đang gửi văn bản cho AI phân tích...",
                <= 15 => "🔍 AI đang kiểm tra chính tả và văn phong...",
                <= 30 => "⚡ AI đang phân tích xung đột nội dung...",
                <= 60 => "📝 AI đang soạn đề xuất cải thiện...",
                _ => "⏳ Đang xử lý văn bản dài, vui lòng chờ..."
            };
        };
        _timer.Start();
    }

    private void StopTimer()
    {
        _timer?.Stop();
        _timer = null;
    }

    private void CopySuggested_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(txtSuggestedContent.Text))
        {
            Clipboard.SetText(txtSuggestedContent.Text);
            MessageBox.Show("📋 Đã copy nội dung đã sửa vào clipboard!",
                "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void CopyResult_Click(object sender, RoutedEventArgs e)
    {
        if (_result == null) return;

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("🔍 KẾT QUẢ KIỂM TRA VĂN BẢN");
        if (!string.IsNullOrWhiteSpace(_title))
            sb.AppendLine($"📄 {_title}");
        sb.AppendLine();
        sb.AppendLine($"📊 Điểm: {_result.OverallScore}/10 — {_result.ScoreText}");
        sb.AppendLine($"🔴 {_result.CriticalCount} Nghiêm trọng  🟡 {_result.WarningCount} Cần xem xét  🟢 {_result.SuggestionCount} Gợi ý");
        sb.AppendLine();
        sb.AppendLine($"📋 Nhận xét: {_result.Summary}");

        // Điểm mạnh
        if (_result.Strengths.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("💪 Điểm mạnh:");
            foreach (var s in _result.Strengths)
                sb.AppendLine($"  ✅ {s}");
        }

        // Vấn đề
        if (_result.Issues.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("⚠️ Các vấn đề:");
            for (int i = 0; i < _result.Issues.Count; i++)
            {
                var issue = _result.Issues[i];
                sb.AppendLine($"  {i + 1}. {issue.SeverityIcon} [{issue.CategoryName}] {issue.Description}");
                if (!string.IsNullOrWhiteSpace(issue.OriginalText))
                    sb.AppendLine($"     Văn bản gốc: {issue.OriginalText}");
                if (!string.IsNullOrWhiteSpace(issue.Suggestion))
                    sb.AppendLine($"     Đề xuất sửa: {issue.Suggestion}");
                if (!string.IsNullOrWhiteSpace(issue.Reason))
                    sb.AppendLine($"     Lý do: {issue.Reason}");
            }
        }

        // Nội dung đã sửa
        if (!string.IsNullOrWhiteSpace(_result.SuggestedContent))
        {
            sb.AppendLine();
            sb.AppendLine("═══════════════════════════════");
            sb.AppendLine("📝 VĂN BẢN SAU KHI SỬA:");
            sb.AppendLine("═══════════════════════════════");
            sb.AppendLine(_result.SuggestedContent);
        }

        Clipboard.SetText(sb.ToString());
        MessageBox.Show("📋 Đã copy toàn bộ kết quả kiểm tra vào clipboard!",
            "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void Apply_Click(object sender, RoutedEventArgs e)
    {
        if (_result != null && !string.IsNullOrWhiteSpace(_result.SuggestedContent))
        {
            var confirm = MessageBox.Show(
                "Bạn có muốn áp dụng nội dung đã sửa vào văn bản?\n\n" +
                "⚠️ Nội dung cũ sẽ bị thay thế.",
                "Xác nhận áp dụng",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirm == MessageBoxResult.Yes)
            {
                AppliedContent = _result.SuggestedContent;
                DialogResult = true;
                Close();
            }
        }
    }

    private void Retry_Click(object sender, RoutedEventArgs e)
    {
        // Quay lại input panel để sửa text và kiểm tra lại
        pnlQuickInput.Visibility = Visibility.Visible;
        pnlLoading.Visibility = Visibility.Collapsed;
        pnlError.Visibility = Visibility.Collapsed;
        pnlResults.Visibility = Visibility.Collapsed;
        scoreBadge.Visibility = Visibility.Collapsed;
        btnApply.Visibility = Visibility.Collapsed;
        btnCopyResult.Visibility = Visibility.Collapsed;
        txtScoreText.Text = "";
    }

    private async void QuickCheck_Click(object sender, RoutedEventArgs e)
    {
        if (!AiPromoHelper.CheckOrShowPromo(this)) return;

        var inputText = txtQuickInput.Text.Trim();
        if (string.IsNullOrWhiteSpace(inputText) || inputText.Length <= 10)
        {
            MessageBox.Show("Vui lòng nhập nội dung văn bản (tối thiểu 10 ký tự).",
                "Thiếu nội dung", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Lấy loại văn bản nếu đã chọn
        _content = inputText;
        if (cboQuickDocType.SelectedValue is string selectedType)
            _documentType = selectedType;

        // Ẩn input panel, hiện loading
        pnlQuickInput.Visibility = Visibility.Collapsed;

        await StartReview();
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}

/// <summary>
/// ViewModel cho hiển thị ReviewIssue trong UI
/// </summary>
public class ReviewIssueViewModel
{
    private readonly ReviewIssue _issue;

    public ReviewIssueViewModel(ReviewIssue issue) => _issue = issue;

    public string SeverityIcon => _issue.SeverityIcon;
    public string Description => _issue.Description;
    public string OriginalText => _issue.OriginalText;
    public string Suggestion => _issue.Suggestion;

    public string CategoryDisplayText => $"{_issue.CategoryIcon} {_issue.CategoryName}";
    public string LocationDisplay => !string.IsNullOrWhiteSpace(_issue.Location) ? $"📍 {_issue.Location}" : "";
    public string ReasonDisplay => !string.IsNullOrWhiteSpace(_issue.Reason) ? $"📖 {_issue.Reason}" : "";

    public Visibility HasOriginalText => !string.IsNullOrWhiteSpace(_issue.OriginalText)
        ? Visibility.Visible : Visibility.Collapsed;
    public Visibility HasSuggestion => !string.IsNullOrWhiteSpace(_issue.Suggestion)
        ? Visibility.Visible : Visibility.Collapsed;
    public Visibility HasReason => !string.IsNullOrWhiteSpace(_issue.Reason)
        ? Visibility.Visible : Visibility.Collapsed;

    // Colors for category badge
    public string CategoryBackground => _issue.CategoryEnum switch
    {
        IssueCategory.Spelling => "#E3F2FD",
        IssueCategory.Style => "#F3E5F5",
        IssueCategory.Conflict => "#FFEBEE",
        IssueCategory.Logic => "#FFF3E0",
        IssueCategory.Missing => "#E8EAF6",
        IssueCategory.Ambiguous => "#FFF8E1",
        IssueCategory.Enhancement => "#E8F5E9",
        IssueCategory.Format => "#FCE4EC",
        _ => "#F5F5F5"
    };

    public string CategoryForeground => _issue.CategoryEnum switch
    {
        IssueCategory.Spelling => "#1565C0",
        IssueCategory.Style => "#7B1FA2",
        IssueCategory.Conflict => "#C62828",
        IssueCategory.Logic => "#E65100",
        IssueCategory.Missing => "#283593",
        IssueCategory.Ambiguous => "#F57F17",
        IssueCategory.Enhancement => "#2E7D32",
        IssueCategory.Format => "#AD1457",
        _ => "#666666"
    };
}
